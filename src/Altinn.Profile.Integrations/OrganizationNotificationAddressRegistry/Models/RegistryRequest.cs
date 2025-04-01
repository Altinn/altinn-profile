using Newtonsoft.Json;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Wrapper class
    /// </summary>
    public record RegistryRequest
    {
        /// <summary>
        /// Gets or sets Contact information
        /// </summary>
        [JsonProperty("Kontaktinformasjon")]
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
        [JsonProperty("bekreftetDato")]
        public string? ConfirmedDate { get; set; }

        /// <summary>
        /// Gets or sets the object containing alert info
        /// </summary>
        [JsonProperty("digitalVarslingsinformasjon")]
        public DigitalContactPointModel? DigitalVarslingsinformasjon { get; set; }

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        [JsonProperty("identifikator")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets message types
        /// </summary>
        [JsonProperty("kanMottaMeldingstype")]
        public List<CanUseModel>? CanReceiveNotificationType { get; set; }

        /// <summary>
        /// Gets or sets the contact information
        /// </summary>
        [JsonProperty("kontaktinformasjonForEnhet")]
        public UnitContactInfoModel? UnitContactInfo { get; set; }
    }
}
