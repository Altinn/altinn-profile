using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddress;

/// <summary>
/// Represents a change log for organizational notification addresses.
/// </summary>
public record NotificationAddressChangesLog
{
    /// <summary>
    /// The collection of snapshots representing the changes to notification addresses of an organization.
    /// </summary>
    [JsonPropertyName("entries")]
    public IList<OrganizationNotificationAddress>? OrganizationNotificationAddressList { get; init; }

    /// <summary>
    /// The title of this change log page.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// The datetime of when the changes were fetched.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTime? Updated { get; init; }

    /// <summary>
    /// The uri for the next batch of data.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public Uri? NextPage { get; init; }
}
