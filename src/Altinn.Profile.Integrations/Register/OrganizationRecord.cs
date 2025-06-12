using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// A record for an organization.
    /// </summary>
    public record OrganizationRecord
    {
        /// <summary>
        /// Gets the organization identifier of the party, or <see langword="null"/> if the party is not an organization.
        /// </summary>
        [JsonPropertyName("organizationIdentifier")]
        public string? OrganizationIdentifier { get; set; }
    }
}
