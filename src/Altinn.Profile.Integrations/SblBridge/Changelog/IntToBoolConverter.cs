using Newtonsoft.Json;

namespace Altinn.Profile.Integrations.SblBridge.Changelog
{
    /// <summary>
    /// Converts between integer values (1/0) and boolean values for JSON serialization and deserialization.
    /// </summary>
    public class IntToBoolConverter : Newtonsoft.Json.JsonConverter
    {
        /// <summary>
        /// Determines whether this converter can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type of the object to check.</param>
        /// <returns><c>true</c> if the object type is <see cref="bool"/>; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }

        /// <summary>
        /// Reads the JSON representation of the object and converts integer values (1/0) or boolean values to a boolean.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. May be <c>null</c>.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>A boolean value based on the JSON input.</returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value) == 1;
            }

            if (reader.TokenType == JsonToken.Boolean)
            {
                // Fix CS8605: Unboxing a possibly null value.
                // Use reader.Value as bool? and check for null before unboxing.
                bool? boolValue = reader.Value as bool?;
                if (boolValue.HasValue)
                {
                    return boolValue.Value;
                }

                throw new JsonSerializationException("Expected non-null boolean value");
            }

            throw new JsonSerializationException("Expected integer or boolean");
        }

        /// <summary>
        /// Writes the JSON representation of a boolean value as an integer (1/0).
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value to write. Should be a boolean or <c>null</c>.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue((value is bool b && b) ? 1 : 0);
        }
    }
}
