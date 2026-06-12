using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.SblBridge.Changelog.Converters
{
    /// <summary>
    /// Converts a JSON string to a nullable DateTime and vice versa.
    /// </summary>
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        /// <summary>
        /// Reads the JSON representation of a nullable DateTime.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">Type of the object.</param>
        /// <param name="options">Serialization options.</param>
        /// <returns>
        /// A <see cref="DateTime"/> value if the string is a valid DateTime; otherwise, <c>null</c>.
        /// </returns>
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] formats =
            {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss.fffffff",
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyy-MM-ddTHH:mm:ss.fffffffK",
            };

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

                if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    return dateTime;
                }

                throw new JsonException($"Invalid DateTime format: {str}");
            }

            return null;
        }

        /// <summary>
        /// Writes the JSON representation of a nullable DateTime.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The value to write. Can be <c>null</c>.</param>
        /// <param name="options">Serialization options.</param>
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStringValue(string.Empty);
            }
            else
            {
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK", CultureInfo.InvariantCulture));
            }
        }
    }
}
