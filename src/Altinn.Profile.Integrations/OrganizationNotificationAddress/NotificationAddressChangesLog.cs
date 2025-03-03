using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddress;

/// <summary>
/// Gets the collection of snapshots representing the changes to notification addresses of an organization.
/// </summary>
public record NotificationAddressChangesLog
{
    /// <summary>
    /// Gets the collection of snapshots representing the changes to notification addresses of an organization.
    /// </summary>
    [JsonPropertyName("entries")]
    public IImmutableList<OrganizationNotificationAddress>? OrganizationNotificationAddressList { get; init; }

    /// <summary>
    /// Gets the title.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the datetime of when the changes were fetched.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTime? Updated { get; init; }

    /// <summary>
    /// Gets the uri for the next batch of data.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public Uri? NextPage { get; init; }
}
