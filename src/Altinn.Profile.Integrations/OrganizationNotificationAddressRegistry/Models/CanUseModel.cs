using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Metadata object
    /// </summary>
    public class CanUseModel
    {
        /// <summary>
        /// Document type
        /// </summary>
        [JsonPropertyName("dokumenttype")]
        public string? DocumentType { get; set; }

        /// <summary>
        /// The service area (tjenesteområde) for this contact point. Currently unused
        /// </summary>
        [JsonPropertyName("tjenesteområde")]
        public string? ServiceArea { get; set; }

        /// <summary>
        /// Trace id
        /// </summary>
        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }
    }
}
