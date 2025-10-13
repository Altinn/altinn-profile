using System.Globalization;

using Newtonsoft.Json;

namespace Altinn.Profile.Integrations.SblBridge.Changelog.Converters
{
    /// <summary>
    /// Converts a JSON string to a nullable DateTime and vice versa.
    /// </summary>
    public class NullableDateTimeConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this converter can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type of the object to check.</param>
        /// <returns><c>true</c> if the object type is nullable <see cref="DateTime"/>; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime?);
        }

        /// <summary>
        /// Reads the JSON representation of a nullable DateTime.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// A <see cref="DateTime"/> value if the string is a valid DateTime; otherwise, <c>null</c>.
        /// </returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            string[] formats =
            {
                "yyyy-MM-dd HH:mm:ss.fff",        // SQL-style
                "yyyy-MM-ddTHH:mm:ss",            // ISO basic
                "yyyy-MM-ddTHH:mm:ss.fff",        // ISO with milliseconds
                "yyyy-MM-ddTHH:mm:ss.fffffff",    // ISO with 7 digits
                "yyyy-MM-ddTHH:mm:ssK",           // ISO with timezone
                "yyyy-MM-ddTHH:mm:ss.fffffffK",   // ISO with high precision + timezone
            };

            if (reader.TokenType == JsonToken.Date)
            {
                return reader.Value;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var str = (string?)reader.Value;
                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }

                if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    return dateTime;
                }

                throw new JsonSerializationException($"Invalid DateTime format: {str}");
            }

            return null;
        }

        /// <summary>
        /// Writes the JSON representation of a nullable DateTime.
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
                writer.WriteValue(((DateTime)value).ToString());
            }
        }
    }
}
