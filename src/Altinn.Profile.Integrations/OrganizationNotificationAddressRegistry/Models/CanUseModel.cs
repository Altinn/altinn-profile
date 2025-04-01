using Newtonsoft.Json;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Metadata object
    /// </summary>
    public class CanUseModel
    {
        /// <summary>
        /// Gets or sets document types
        /// </summary>
        [JsonProperty("dokumenttype")]
        public string? DocumentType { get; set; }

        /// <summary>
        /// Gets or sets an unknown field
        /// </summary>
        [JsonProperty("tjenesteområde")]
        public string? ServiceArea { get; set; }

        /// <summary>
        /// Gets or sets trace id
        /// </summary>
        [JsonProperty("traceId")]
        public string? TraceId { get; set; }
    }
}
