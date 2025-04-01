using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// The identifier of the unit of the contact point.
    /// </summary>
    public record UnitContactInfoModel
    {
        /// <summary>
        /// The identifier of the unit of the contact point.
        /// </summary>
        [JsonPropertyName("enhetsidentifikator")]
        public UnitIdentifierModel? UnitIdentifier { get; init; }
    }

    /// <summary>
    /// The identifier of the unit.
    /// </summary>
    public record UnitIdentifierModel
    {
        /// <summary>
        /// The value of the identifier of the contact point.
        /// </summary>
        [JsonPropertyName("verdi")]
        public string? Value { get; init; }

        /// <summary>
        /// The type of identifier.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}
