using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OfficialAddressRegister;

/// <summary>
/// Gets the collection of snapshots representing the changes to offical addresses of a unit.
/// </summary>
public record OfficialAddressRegisterChangesLog
{
    /// <summary>
    /// Gets the collection of snapshots representing the changes to offical addresses of a unit.
    /// </summary>
    [JsonPropertyName("entries")]
    public IImmutableList<OfficialAddress>? OfficialAddressList { get; init; }

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
