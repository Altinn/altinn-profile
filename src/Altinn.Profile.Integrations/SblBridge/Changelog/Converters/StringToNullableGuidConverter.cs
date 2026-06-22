using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.SblBridge.Changelog.Converters
{
    /// <summary>
    /// Converts a JSON string to a nullable Guid and vice versa.
    /// </summary>
    public class StringToNullableGuidConverter : JsonConverter<Guid?>
    {
        /// <summary>
        /// Reads the JSON representation of a nullable Guid.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">Type of the object.</param>
        /// <param name="options">Serialization options.</param>
        /// <returns>
        /// A <see cref="Guid"/> value if the string is a valid Guid; otherwise, <c>null</c>.
        /// </returns>
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }

                if (Guid.TryParse(str, out var guid))
                {
                    return guid;
                }

                throw new JsonException($"Invalid Guid format: {str}");
            }

            return null;
        }

        /// <summary>
        /// Writes the JSON representation of a nullable Guid.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The value to write. Can be <c>null</c>.</param>
        /// <param name="options">Serialization options.</param>
        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStringValue(string.Empty);
            }
            else
            {
                writer.WriteStringValue(value.Value.ToString());
            }
        }
    }
}
