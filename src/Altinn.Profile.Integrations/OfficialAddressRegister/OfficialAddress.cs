using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OfficialAddressRegister
{
    /// <summary>
    /// Gets the changes to an official contact point
    /// </summary>
    public record OfficialAddress
    {
        /// <summary>
        /// Gets the title of the cotanct point.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        /// <summary>
        /// Gets the identificator of the cotanct point.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>
        /// Gets the date and time when the contact point was updated.
        /// </summary>
        [JsonPropertyName("updated")]
        public DateTime? Updated { get; init; }

        /// <summary>
        /// Gets the mobile title of the cotanct point.
        /// </summary>
        [JsonPropertyName("isdeleted")]
        public bool? IsDeleted { get; init; }

        /// <summary>
        /// Gets the content of the contact point.
        /// </summary>
        [JsonPropertyName("content")]
        public string? ContentStringified { get; init; }
    }
}
