using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Used for sync return values
    /// </summary>
    public record RegistryResponse
    {
        /// <summary>
        /// Sync status
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>
        /// A value indicating whether the success or failure of the request
        /// </summary>
        [JsonPropertyName("boolResult")]
        public bool? BoolResult { get; init; }

        /// <summary>
        /// TraceID of the response
        /// </summary>
        [JsonPropertyName("traceId")]
        public string? TraceId { get; init; }

        /// <summary>
        /// Details
        /// </summary>
        [JsonPropertyName("details")]
        public string? Details { get; init; }
    }
}
