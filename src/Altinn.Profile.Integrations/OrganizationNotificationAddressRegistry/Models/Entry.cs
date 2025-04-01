using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

/// <summary>
/// Represents changes to a notification address for an organization
/// </summary>
public record Entry
{
    /// <summary>
    /// The title of the notification address.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// The identificator of the notification address.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// The date and time when the notification address was updated.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTime Updated { get; init; }

    /// <summary>
    /// Gets if the notification address is deleted.
    /// </summary>
    [JsonPropertyName("isdeleted")]
    public bool? IsDeleted { get; init; }

    /// <summary>
    /// The content of the notification address as a serialized string. Will be null if the address is marked as deleted.
    /// </summary>
    [JsonPropertyName("content")]
    public string? ContentStringified { get; init; }

    /// <summary>
    /// The content of the notification address. Will be null if the address is marked as deleted.
    /// </summary>
    [JsonIgnore]
    public EntryContent? Content => ContentStringified != null ? JsonSerializer.Deserialize<EntryContent>(ContentStringified) : null;
}
