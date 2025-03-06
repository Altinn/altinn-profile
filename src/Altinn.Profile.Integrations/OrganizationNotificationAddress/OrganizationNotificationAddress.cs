using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddress
{
    /// <summary>
    /// Represents changes to a notification address for an organization
    /// </summary>
    public record OrganizationNotificationAddress
    {
        /// <summary>
        /// Gets the title of the notification address.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        /// <summary>
        /// Gets the identificator of the notification address.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>
        /// Gets the date and time when the notification address was updated.
        /// </summary>
        [JsonPropertyName("updated")]
        public DateTime? Updated { get; init; }

        /// <summary>
        /// Gets if the notification address is deleted.
        /// </summary>
        [JsonPropertyName("isdeleted")]
        public bool? IsDeleted { get; init; }

        /// <summary>
        /// Gets the content of the notification address as a serialized string.
        /// </summary>
        [JsonPropertyName("content")]
        public string? ContentStringified { get; init; }

        /// <summary>
        /// Gets the content of the notification address.
        /// </summary>
        [JsonIgnore] 
        public EntryContent? Content => ContentStringified != null ? JsonSerializer.Deserialize<EntryContent>(ContentStringified) : throw new ArgumentNullException("Content");
    }
}
