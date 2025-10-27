using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Core.Utils
{
    /// <summary>
    /// Converts <see cref="Optional{T}"/> values to and from JSON, handling explicit nulls and missing values.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the <see cref="Optional{T}"/>.</typeparam>
    public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
    {
        /// <summary>
        /// Reads and converts the JSON to an <see cref="Optional{T}"/> object.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>An <see cref="Optional{T}"/> instance.</returns>
        public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle explicit null
            if (reader.TokenType == JsonTokenType.Null)
            {
                // Return an Optional<T> with HasValue = true, Value = null
                return new Optional<T>(value: default);
            }

            // Deserialize normally
            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return new Optional<T>(value);
        }

        /// <summary>
        /// Writes an <see cref="Optional{T}"/> object as JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }

    /// <summary>
    /// A factory for creating <see cref="OptionalJsonConverter{T}"/> instances for <see cref="Optional{T}"/> types.
    /// </summary>
    public class OptionalJsonConverterFactory : JsonConverterFactory
    {
        /// <summary>
        /// Determines whether the specified type can be converted by this factory.
        /// </summary>
        /// <param name="typeToConvert">The type to check.</param>
        /// <returns><c>true</c> if the type is an <see cref="Optional{T}"/>; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

        /// <summary>
        /// Creates a converter for the specified type.
        /// </summary>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A <see cref="JsonConverter"/> for the specified type.</returns>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(innerType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }
}
