using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Wrapper class for contact information to be sent to the registry.
    /// This structure aligns with the external API requirements for the organization notification address registry.
    /// </summary>
    public record RegistryRequest
    {
        /// <summary>
        /// Gets or sets Contact information
        /// </summary>
        [JsonPropertyName("Kontaktinformasjon")]
        public ContactInfoModel? ContactInfo { get; set; }
    }

    /// <summary>
    /// External model for contact info
    /// </summary>
    public record ContactInfoModel
    {
        /// <summary>
        /// ConfirmedDate
        /// </summary>
        [JsonPropertyName("bekreftetDato")]
        public string? ConfirmedDate { get; set; }

        /// <summary>
        /// A channel for digital contact, either email or phone
        /// </summary>
        [JsonPropertyName("digitalVarslingsinformasjon")]
        public DigitalContactPointModel? DigitalContactPoint { get; set; }

        /// <summary>
        /// The identifier in the registry
        /// </summary>
        [JsonPropertyName("identifikator")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets message types
        /// </summary>
        [JsonPropertyName("kanMottaMeldingstype")]
        public List<CanUseModel>? CanReceiveNotificationType { get; set; }

        /// <summary>
        /// Gets or sets the contact information
        /// </summary>
        [JsonPropertyName("kontaktinformasjonForEnhet")]
        public UnitContactInfoModel? UnitContactInfo { get; set; }
    }
}
