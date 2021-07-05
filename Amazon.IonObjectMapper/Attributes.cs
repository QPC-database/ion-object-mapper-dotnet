using System;

namespace Amazon.IonObjectMapper
{
    /// <summary>
    /// Attribute to identify a .NET property that should be ignored
    /// by the serializer.
    /// </summary>
    public class IonIgnore : Attribute
    {
    }

    /// <summary>
    /// Attribute to identify a .NET type to target during deserialization.
    /// </summary>
    public class IonAnnotateType : Attribute
    {
        /// <summary>
        /// Flag to indicate that whether any classes descending from the annotated class
        /// are excluded from the annotation.
        /// </summary>
        public bool ExcludeDescendants { get; init; }
        
        /// <summary>
        /// The .NET namespace of the annotated type.
        /// </summary>
        public string Prefix { get; set; }
        
        /// <summary>
        /// The .NET class name of the annotated type.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Attribute to identify a .NET type that should not be annotated
    /// even if a parent class is annotated.
    /// </summary>
    public class IonDoNotAnnotateType : Attribute
    {
        /// <summary>
        /// Flag to indicate that whether any classes descending from the annotated class
        /// are excluded from the IonAnnotateType annotation.
        /// </summary>
        public bool ExcludeDescendants { get; init; }
    }

    /// <summary>
    /// Attribute to identify a custom Ion Serializer to be used to serialize
    /// and deserialize instances of the class annotated with this attribute.
    /// </summary>
    public class IonSerializerAttribute : Attribute
    {
        /// <summary>
        /// The name of the IonSerializer Attribute to be used 
        /// to create IonSerializerFactory with custom context
        /// </summary>
        public Type Factory { get; init; }
        
        /// <summary>
        /// The name of the IonSerializer Attribute to be used 
        /// to create custom IonSerializer
        /// </summary>
        public Type Serializer { get; init; }
    }

    /// <summary>
    /// Attribute to identify a custom constructor to be used to instantiate instances
    /// of classes annotated with this attribute during deserialization.
    /// </summary>
    public class IonConstructor : Attribute
    {
    }

    /// <summary>
    /// Attribute to identify the Ion field name to be used during serialization and/or
    /// deserialization of a .NET property annotated with this attribute.
    /// </summary>
    public class IonPropertyName : Attribute
    {
        /// <summary>
        /// IonPropertyName constructor.
        /// </summary>
        public IonPropertyName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The Ion field name to be used instead of the .NET property's name.
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Attribute to identify an Ion property getter method to be used
    /// during serialization of that Ion property.
    /// </summary>
    public class IonPropertyGetter : Attribute
    {
        /// <summary>
        /// IonPropertyGetter constructor.
        /// </summary>
        public IonPropertyGetter(string ionPropertyName)
        {
            this.IonPropertyName = ionPropertyName;
        }

        /// <summary>
        /// The name of the Ion property to be serialized with the getter method.
        /// </summary>
        public string IonPropertyName { get; }
    }

    /// <summary>
    /// Attribute to identify an Ion property setter method to be used
    /// during deserialization of that Ion property.
    /// </summary>
    public class IonPropertySetter : Attribute
    {
        /// <summary>
        /// IonPropertySetter constructor.
        /// </summary>
        public IonPropertySetter(string ionPropertyName)
        {
            IonPropertyName = ionPropertyName;
        }

        /// <summary>
        /// The name of the Ion property to be deserialized with the setter method.
        /// </summary>
        public string IonPropertyName { get; }
    }

    /// <summary>
    /// Attribute to identify a .NET field to be included during
    /// serialization and deserialization.
    /// </summary>
    public class IonField : Attribute
    {
    }
}