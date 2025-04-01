using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

/// <summary>
/// The content of the notification address.
/// </summary>
public record EntryContent
{
    /// <summary>
    /// The content of the contact point.
    /// </summary>
    [JsonPropertyName("Kontaktinformasjon")]
    public ContactPointModel? ContactPoint { get; init; }

    /// <summary>
    /// The content of the contact point.
    /// </summary>
    public record ContactPointModel
    {
        /// <summary>
        /// The identificator of the contact point.
        /// </summary>
        [JsonPropertyName("identifikator")]
        public string? Id { get; init; }

        /// <summary>
        /// The content of the contact point.
        /// </summary>
        [JsonPropertyName("digitalVarslingsinformasjon")]
        public DigitalContactPointModel? DigitalContactPoint { get; init; }

        /// <summary>
        /// The content of the contact point.
        /// </summary>
        [JsonPropertyName("kontaktinformasjonForEnhet")]
        public UnitContactInfoModel? UnitContactInfo { get; init; }
    }
}
