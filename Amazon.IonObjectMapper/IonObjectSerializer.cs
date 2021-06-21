﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.IonDotnet;

namespace Amazon.IonObjectMapper
{
    public class IonObjectSerializer : IonSerializer<object>
    {
        private const BindingFlags BINDINGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        private readonly IonSerializer ionSerializer;
        private readonly IonSerializationOptions options;
        private readonly Type targetType;
        private readonly Lazy<IEnumerable<PropertyInfo>> readOnlyProperties;

        public IonObjectSerializer(IonSerializer ionSerializer, IonSerializationOptions options, Type targetType)
        {
            this.ionSerializer = ionSerializer;
            this.options = options;
            this.targetType = targetType;
            this.readOnlyProperties = new Lazy<IEnumerable<PropertyInfo>>(
                () => this.targetType.GetProperties().Where(IsReadOnlyProperty));
        }

        public override object Deserialize(IIonReader reader)
        {
            var ionConstructors = targetType.GetConstructors(BINDINGS).Where(IsIonConstructor).Take(2);
            if (ionConstructors.Any())
            {
                if (ionConstructors.Count() > 1)
                {
                    throw new InvalidOperationException(
                        $"Only one constructor in class {targetType.Name} may be annotated " +
                        "with the [IonConstructor] attribute and more than one was detected");
                }

                return this.DeserializeWithIonConstructor(ionConstructors.First(), reader);
            }

            var targetObject = options.ObjectFactory.Create(options, reader, targetType);
            reader.StepIn();

            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                MethodInfo method;
                PropertyInfo property;
                FieldInfo field;

                // Check if current Ion field has a IonPropertySetter annotated setter method.
                if ((method = FindSetter(reader.CurrentFieldName)) != null)
                {
                    var deserialized = new object();
                    if (this.TryDeserializeMethod(method, reader, ionType, ref deserialized))
                    {
                        method.Invoke(targetObject, new[]{ deserialized });
                    }
                }
                // Check if current Ion field is a .NET property.
                else if ((property = FindProperty(reader.CurrentFieldName)) != null)
                {
                    var deserialized = new object();
                    if (this.TryDeserializeProperty(property, reader, ionType, ref deserialized))
                    {
                        property.SetValue(targetObject, deserialized);
                    }
                }
                // Check if current Ion field is a .NET field.
                else if ((field = FindField(reader.CurrentFieldName)) != null)
                {
                    var deserialized = new object();
                    if (this.TryDeserializeField(field, reader, ionType, ref deserialized))
                    {
                        field.SetValue(targetObject, deserialized);
                    }
                }
            }
            reader.StepOut();
            return targetObject;
        }

        public override void Serialize(IIonWriter writer, object item)
        {
            options.TypeAnnotator.Apply(options, writer, targetType);
            writer.StepIn(IonType.Struct);

            var serializedIonFields = new HashSet<string>();
            
            // Serialize the values returned from IonPropertyGetter annotated getter methods.
            foreach (var (method, ionPropertyName) in this.GetGetters())
            {
                var getValue = method.Invoke(item, Array.Empty<object>());
                
                writer.SetFieldName(ionPropertyName);
                ionSerializer.Serialize(writer, getValue);
                
                serializedIonFields.Add(ionPropertyName);
            }

            // Serialize any properties that satisfy the options/attributes.
            foreach (var property in targetType.GetProperties())
            {
                var ionPropertyName = IonFieldNameFromProperty(property);
                if (serializedIonFields.Contains(ionPropertyName))
                {
                    // This Ion property name was already serialized.
                    continue;
                }
                
                if (property.GetCustomAttributes(true).Any(it => it is IonIgnore))
                {
                    continue;
                }

                if (this.options.IgnoreReadOnlyProperties && IsReadOnlyProperty(property))
                {
                    continue;
                }

                var propertyValue = property.GetValue(item);
                if (options.IgnoreNulls && propertyValue == null)
                {
                    continue;
                }
                if (options.IgnoreDefaults && propertyValue == default)
                {
                    continue;
                }

                writer.SetFieldName(ionPropertyName);
                ionSerializer.Serialize(writer, propertyValue);
            }

            // Serialize any fields that satisfy the options/attributes.
            foreach (var field in Fields())
            {
                var ionFieldName = GetFieldName(field);
                if (serializedIonFields.Contains(ionFieldName))
                {
                    // This Ion field name was already serialized.
                    continue;
                }
                
                if (options.IgnoreReadOnlyFields && field.IsInitOnly)
                {
                    continue;
                }

                var fieldValue = field.GetValue(item);
                if (options.IgnoreNulls && fieldValue == null)
                {
                    continue;
                }
                if (options.IgnoreDefaults && fieldValue == default)
                {
                    continue;
                }

                writer.SetFieldName(ionFieldName);
                ionSerializer.Serialize(writer, fieldValue);
            }

            writer.StepOut();
        }

        // Deserialize the given method and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeMethod(MethodInfo method, IIonReader reader, IonType ionType, ref object deserialized)
        {
            // A setter should have exactly one argument.
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                throw new InvalidOperationException(
                    "An [IonPropertySetter] annotated method should have exactly one argument " +
                    $"but {method.Name} has {parameters.Length} arguments");
            }

            deserialized = ionSerializer.Deserialize(reader, parameters[0].ParameterType, ionType);

            return true;
        }
        
        // Deserialize the given property and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeProperty(
            PropertyInfo property, IIonReader reader, IonType ionType, ref object deserialized)
        {
            // We deserialize whether or not we ultimately use the result because values
            // for some Ion types need to be consumed in order to advance the reader.
            deserialized = ionSerializer.Deserialize(reader, property.PropertyType, ionType);
            
            if (IsReadOnlyProperty(property))
            {
                // property.SetValue() does not work with a readonly property.
                // logic for handling deserializing readonly properties happens during field processing
                // when we detect backing fields for the property.
                return false;
            }

            return !options.IgnoreDefaults || deserialized != default;
        }
        
        // Deserialize the given field and return bool to indicate whether the deserialized result should be used.
        private bool TryDeserializeField(FieldInfo field, IIonReader reader, IonType ionType, ref object deserialized)
        {
            // We deserialize whether or not we ultimately use the result because values
            // for some Ion types need to be consumed in order to advance the reader.
            deserialized = ionSerializer.Deserialize(reader, field.FieldType, ionType);
            
            if (options.IgnoreReadOnlyFields && field.IsInitOnly)
            {
                return false;
            }

            return !options.IgnoreDefaults || deserialized != default;
        }

        private object DeserializeWithIonConstructor(ConstructorInfo ionConstructor, IIonReader reader)
        {
            var parameters = ionConstructor.GetParameters();

            // Compute mapping between parameter names and index in parameter array so we can figure out the
            // correct order of the constructor arguments.
            var paramIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var ionPropertyName = (IonPropertyName) parameters[i].GetCustomAttribute(typeof(IonPropertyName));
                if (ionPropertyName == null)
                {
                    throw new NotSupportedException(
                        $"Parameter '{parameters[i].Name}' is not specified with the [IonPropertyName] attribute " +
                        $"for {targetType.Name}'s IonConstructor. All constructor arguments must be annotated " +
                        "so we know which parameters to set at construction time.");
                }

                paramIndexMap.Add(ionPropertyName.Name, i);
            }

            reader.StepIn();

            // Deserialize Ion and organize deserialized results into four categories:
            // 1. Values to be passed into the Ion constructor.
            // 2. Values to be set via annotated methods after construction.
            // 3. Properties to be set after construction.
            // 4. Fields to be set after construction.
            var constructorArgs = new object[parameters.Length];
            var setterMethods = new List<(MethodInfo, object)>();
            var remainingProperties = new List<(PropertyInfo, object)>();
            var remainingFields = new List<(FieldInfo, object)>();
            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                MethodInfo method;
                PropertyInfo property;
                FieldInfo field;
                if (paramIndexMap.ContainsKey(reader.CurrentFieldName))
                {
                    var index = paramIndexMap[reader.CurrentFieldName];
                    var deserialized = ionSerializer.Deserialize(reader, parameters[index].ParameterType, ionType);
                    constructorArgs[index] = deserialized;
                }
                else if ((method = FindSetter(reader.CurrentFieldName)) != null)
                {
                    var deserialized = new object();
                    if (this.TryDeserializeMethod(method, reader, ionType, ref deserialized))
                    {
                        setterMethods.Add((method, deserialized));
                    }
                }
                else if ((property = FindProperty(reader.CurrentFieldName)) != null)
                {
                    var deserialized = new object();
                    if (this.TryDeserializeProperty(property, reader, ionType, ref deserialized))
                    {
                        remainingProperties.Add((property, deserialized));
                    }
                }
                else if ((field = FindField(reader.CurrentFieldName)) != null)
                {
                    var deserialized = new object();
                    if (this.TryDeserializeField(field, reader, ionType, ref deserialized))
                    {
                        remainingFields.Add((field, deserialized));
                    }
                }
            }

            reader.StepOut();

            var targetObject = ionConstructor.Invoke(constructorArgs);

            // Set values with annotated methods.
            foreach (var (method, deserialized) in setterMethods)
            {
                method.Invoke(targetObject, new[]{ deserialized });
            }
            
            // Set remaining properties/fields.
            foreach (var (property, deserialized) in remainingProperties)
            {
                property.SetValue(targetObject, deserialized);
            }
            foreach (var (field, deserialized) in remainingFields)
            {
                field.SetValue(targetObject, deserialized);
            }

            return targetObject;
        }

        private IEnumerable<(MethodInfo, string)> GetGetters()
        {
            var getters = new List<(MethodInfo, string)>();
            foreach (var method in targetType.GetMethods())
            {
                var getMethod = (IonPropertyGetter)method.GetCustomAttribute(typeof(IonPropertyGetter));
                
                // A getter method should have zero parameters.
                if (getMethod?.IonPropertyName == null || method.GetParameters().Length != 0)
                {
                    continue;
                }
                
                getters.Add((method, getMethod.IonPropertyName));
            }

            return getters;
        }
        
        private MethodInfo FindSetter(string name)
        {
            return targetType.GetMethods().FirstOrDefault(m =>
            {
                var setMethod = (IonPropertySetter)m.GetCustomAttribute(typeof(IonPropertySetter));
                return setMethod != null && setMethod.IonPropertyName == name;
            });
        }

        private string IonFieldNameFromProperty(PropertyInfo property)
        {
            var ionPropertyName = property.GetCustomAttribute(typeof(IonPropertyName));
            if (ionPropertyName != null) 
            {
                return ((IonPropertyName)ionPropertyName).Name;
            }

            return options.NamingConvention.FromProperty(property.Name);
        }

        private PropertyInfo FindProperty(string readName)
        {
            var exact = IonNamedProperties().FirstOrDefault(p => 
                {
                    var ionPropertyName = p.GetCustomAttribute<IonPropertyName>();
                    if (ionPropertyName != null)
                    {
                        return p.GetCustomAttribute<IonPropertyName>().Name == readName;
                    }
                    return false;
                });
            if (exact != null)
            {
                return exact;
            }

            if (options.PropertyNameCaseInsensitive)
            {
                return targetType.GetProperties().FirstOrDefault(p => String.Equals(p.Name, readName, StringComparison.OrdinalIgnoreCase));
            }

            var name = options.NamingConvention.ToProperty(readName);
            var property = targetType.GetProperty(name, BINDINGS);

            return property;
        }

        private bool IsReadOnlyProperty(PropertyInfo property)
        {
            return property.SetMethod == null;
        }
        
        private FieldInfo FindField(string name)
        {
            var exact = targetType.GetField(name, BINDINGS);
            if (exact != null && IsField(exact))
            {
                return exact;
            }

            return Fields().FirstOrDefault(f => 
            {
                var propertyName = f.GetCustomAttribute(typeof(IonPropertyName));
                if (propertyName != null)
                {
                    return name == ((IonPropertyName)propertyName).Name;
                }
                return false;
            });
        }

        private static bool IsIonConstructor(ConstructorInfo constructor)
        {
            return constructor.GetCustomAttribute(typeof(IonConstructor)) != null;
        }

        private static bool IsIonField(FieldInfo field)
        {
            return field.GetCustomAttribute(typeof(IonField)) != null;
        }

        private static bool IsIonNamedProperty(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(IonPropertyName)) != null;
        }
        
        private bool IsField(FieldInfo field)
        {
            if (options.IncludeFields)
            {
                return true;
            }

            if (!this.options.IgnoreReadOnlyProperties &&
                this.readOnlyProperties.Value.Any(p => field.Name == $"<{p.Name}>k__BackingField"))
            {
                return true;
            }

            return IsIonField(field);
        }

        private IEnumerable<FieldInfo> Fields()
        {
            return targetType.GetFields(BINDINGS).Where(IsField);
        }

        private IEnumerable<PropertyInfo> IonNamedProperties()
        {
            return targetType.GetProperties().Where(IsIonNamedProperty);
        }

        private string GetFieldName(FieldInfo field)
        {
            var propertyName = field.GetCustomAttribute(typeof(IonPropertyName));
            if (propertyName != null)
            {
                return ((IonPropertyName)propertyName).Name;
            }
            return field.Name;
        }
    }
}
