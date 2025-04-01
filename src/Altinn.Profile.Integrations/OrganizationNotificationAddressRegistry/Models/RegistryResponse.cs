using Newtonsoft.Json;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Used for sync return values
    /// </summary>
    public record RegistryResponse
    {
        /// <summary>
        /// Gets or sets sync status
        /// </summary>
        [JsonProperty("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the success or failure of the request
        /// </summary>
        [JsonProperty("boolResult")]
        public bool? BoolResult { get; set; }

        /// <summary>
        /// Gets or sets details
        /// </summary>
        [JsonProperty("details")]
        public string? Details { get; set; }
    }
}
