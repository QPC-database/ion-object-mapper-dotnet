using System;
using System.Collections.Generic; 
using System.Numerics;
using System.Text;
using Amazon.IonDotnet;

namespace Amazon.IonObjectMapper
{
    /// <summary>
    /// Serializer for serializing and deserializing null values.
    /// </summary>
    public class IonNullSerializer : IonSerializer<object>
    {
        /// <summary>
        /// Deserialize null value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized null value.</returns>
        public override object Deserialize(IIonReader reader)
        {
            return null;
        }

        /// <summary>
        /// Serialize null value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The null object to serialize.</param>
        public override void Serialize(IIonWriter writer, object item)
        {
            writer.WriteNull();
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing byte arrays.
    /// </summary>
    public class IonByteArraySerializer : IonSerializer<byte[]>
    {
        /// <summary>
        /// Deserialize byte array.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized byte array.</returns>
        public override byte[] Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return blob;
        }

        /// <summary>
        /// Serialize byte array.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The byte array to serialize.</param>
        public override void Serialize(IIonWriter writer, byte[] item)
        {
            writer.WriteBlob(item);
        }
    }
    
    /// <summary>
    /// Serializer for serializing and deserializing string values.
    /// </summary>
    public class IonStringSerializer : IonSerializer<string>
    {
        /// <summary>
        /// Deserialize string value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized string value.</returns>
        public override string Deserialize(IIonReader reader)
        {
            return reader.StringValue();
        }

        /// <summary>
        /// Serialize string value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The string value to serialize.</param>
        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteString(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing int values.
    /// </summary>
    public class IonIntSerializer : IonSerializer<int>
    {
        /// <summary>
        /// Deserialize int value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized int value.</returns>
        public override int Deserialize(IIonReader reader)
        {
            return reader.IntValue();
        }

        /// <summary>
        /// Serialize int value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The int value to serialize.</param>
        public override void Serialize(IIonWriter writer, int item)
        {
            writer.WriteInt(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing long values.
    /// </summary>
    public class IonLongSerializer : IonSerializer<long>
    {
        internal static readonly string ANNOTATION = "numeric.int32";

        /// <summary>
        /// Deserialize long value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized long value.</returns>
        public override long Deserialize(IIonReader reader)
        {
            return reader.IntValue();
        }

        /// <summary>
        /// Serialize long value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The long value to serialize.</param>
        public override void Serialize(IIonWriter writer, long item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteInt(item);
        }
    }
    
    /// <summary>
    /// Serializer for serializing and deserializing boolean values.
    /// </summary>
    public class IonBooleanSerializer : IonSerializer<bool>
    {
        /// <summary>
        /// Deserialize boolean value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized boolean value.</returns>
        public override bool Deserialize(IIonReader reader)
        {
            return reader.BoolValue();
        }

        /// <summary>
        /// Serialize boolean value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The boolean value to serialize.</param>
        public override void Serialize(IIonWriter writer, bool item)
        {
            writer.WriteBool(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing double values.
    /// </summary>
    public class IonDoubleSerializer : IonSerializer<double>
    {
        /// <summary>
        /// Deserialize double value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized double value.</returns>
        public override double Deserialize(IIonReader reader)
        {
            return reader.DoubleValue();
        }

        /// <summary>
        /// Serialize double value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The double value to serialize.</param>
        public override void Serialize(IIonWriter writer, double item)
        {
            writer.WriteFloat(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing decimal values.
    /// </summary>
    public class IonDecimalSerializer : IonSerializer<decimal>
    {
        internal static readonly string ANNOTATION = "numeric.decimal128";

        /// <summary>
        /// Deserialize decimal value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized decimal value.</returns>
        public override decimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue().ToDecimal();
        }

        /// <summary>
        /// Serialize decimal value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The decimal value to serialize.</param>
        public override void Serialize(IIonWriter writer, decimal item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteDecimal(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing big decimal values.
    /// </summary>
    public class IonBigDecimalSerializer : IonSerializer<BigDecimal>
    {
        /// <summary>
        /// Deserialize big decimal value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized big decimal value.</returns>
        public override BigDecimal Deserialize(IIonReader reader)
        {
            return reader.DecimalValue();
        }

        /// <summary>
        /// Serialize big decimal value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The big decimal value to serialize.</param>
        public override void Serialize(IIonWriter writer, BigDecimal item)
        {
            writer.WriteDecimal(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing float values.
    /// </summary>
    public class IonFloatSerializer : IonSerializer<float>
    {
        internal static readonly string ANNOTATION = "numeric.float32";

        /// <summary>
        /// Deserialize float value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized float value.</returns>
        public override float Deserialize(IIonReader reader)
        {
            
            return Convert.ToSingle(reader.DoubleValue());
        }

        /// <summary>
        /// Serialize float value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The float value to serialize.</param>
        public override void Serialize(IIonWriter writer, float item)
        {
            writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            writer.WriteFloat(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing date time values.
    /// </summary>
    public class IonDateTimeSerializer : IonSerializer<DateTime>
    {
        /// <summary>
        /// Deserialize date time value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized date time value.</returns>
        public override DateTime Deserialize(IIonReader reader)
        {
            return reader.TimestampValue().DateTimeValue;
        }

        /// <summary>
        /// Serialize date time value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The date time value to serialize.</param>
        public override void Serialize(IIonWriter writer, DateTime item)
        {
            writer.WriteTimestamp(new Timestamp(item));
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing Symbol Token values.
    /// </summary>
    public class IonSymbolSerializer : IonSerializer<SymbolToken>
    {
        /// <summary>
        /// Deserialize Symbol Token value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized Symbol Token value.</returns>
        public override SymbolToken Deserialize(IIonReader reader)
        {
            return reader.SymbolValue();
        }

        /// <summary>
        /// Serialize Symbol Token value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The Symbol Token value to serialize.</param>
        public override void Serialize(IIonWriter writer, SymbolToken item)
        {
            writer.WriteSymbolToken(item);
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing CLOB values.
    /// </summary>
    public class IonClobSerializer : IonSerializer<string>
    {
        /// <summary>
        /// Deserialize CLOB value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized CLOB value.</returns>
        public override string Deserialize(IIonReader reader)
        {
            byte[] clob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(clob);
            return Encoding.UTF8.GetString(clob);
        }

        /// <summary>
        /// Serialize CLOB value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The CLOB value to serialize.</param>
        public override void Serialize(IIonWriter writer, string item)
        {
            writer.WriteClob(Encoding.UTF8.GetBytes(item));
        }
    }

    /// <summary>
    /// Serializer for serializing and deserializing Guid values.
    /// </summary>
    public class IonGuidSerializer : IonSerializer<Guid>
    {
        internal static readonly string ANNOTATION = "guid128";
        private IonSerializationOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonGuidSerializer"/> class.
        /// </summary>
        ///
        /// <param name="options">Serialization options.</param>
        public IonGuidSerializer(IonSerializationOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Deserialize Guid value.
        /// </summary>
        ///
        /// <param name="reader">The Ion reader used during deserialization.</param>
        ///
        /// <returns>The deserialized Guid value.</returns>
        public override Guid Deserialize(IIonReader reader)
        {
            byte[] blob = new byte[reader.GetLobByteSize()];
            reader.GetBytes(blob);
            return new Guid(blob);
        }

        /// <summary>
        /// Serialize Guid value.
        /// </summary>
        ///
        /// <param name="writer">The Ion writer used during serialization.</param>
        /// <param name="item">The Guid value to serialize.</param>
        public override void Serialize(IIonWriter writer, Guid item)
        {
            if (options.AnnotateGuids) {
                writer.SetTypeAnnotations(new List<string>() { ANNOTATION });
            }
            writer.WriteBlob(item.ToByteArray());
        }
    }
}