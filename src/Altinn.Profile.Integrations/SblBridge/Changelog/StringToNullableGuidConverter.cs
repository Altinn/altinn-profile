using Newtonsoft.Json;

namespace Altinn.Profile.Integrations.SblBridge.Changelog
{
    /// <summary>
    /// Converts a JSON string to a nullable Guid and vice versa.
    /// </summary>
    public class StringToNullableGuidConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this converter can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type of the object to check.</param>
        /// <returns><c>true</c> if the object type is <see cref="Guid?"/>; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid?);
        }

        /// <summary>
        /// Reads the JSON representation of a nullable Guid.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// A <see cref="Guid"/> value if the string is a valid Guid; otherwise, <c>null</c>.
        /// </returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var str = (string?)reader.Value;
                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }

                if (Guid.TryParse(str, out var guid))
                {
                    return guid;
                }

                throw new JsonSerializationException($"Invalid Guid format: {str}");
            }

            return null;
        }

        /// <summary>
        /// Writes the JSON representation of a nullable Guid.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value to write. Can be <c>null</c>.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(string.Empty);
            }
            else
            {
                writer.WriteValue(((Guid)value).ToString());
            }
        }
    }
}
