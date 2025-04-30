using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Used for sync return values
    /// </summary>
    public record RegistryResponse
    {
        /// <summary>
        /// Sync status, eg "OK", "VALIDATION_ERROR"
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>
        /// A value indicating whether the request was handled as a success or failure
        /// </summary>
        [JsonPropertyName("boolResult")]
        public bool? BoolResult { get; init; }

        /// <summary>
        /// Id of the address in the registry of the response
        /// </summary>
        [JsonPropertyName("addressId")]
        public string? AddressID { get; init; }

        /// <summary>
        /// TraceID of the response
        /// </summary>
        [JsonPropertyName("traceId")]
        public string? TraceId { get; init; }

        /// <summary>
        /// Details of the error if there is a validation error
        /// </summary>
        [JsonPropertyName("details")]
        public string? Details { get; init; }
    }
}
