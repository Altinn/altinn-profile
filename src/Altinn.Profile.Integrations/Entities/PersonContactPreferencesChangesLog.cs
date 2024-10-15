using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a log of changes to a person's contact preferences.
/// </summary>
public class PersonContactPreferencesChangesLog : IPersonContactPreferencesChangesLog
{
    /// <summary>
    /// Gets the list of snapshots representing the changes to the person's contact preferences.
    /// </summary>
    /// <value>A collection of <see cref="PersonContactPreferencesSnapshot"/> objects.</value>
    [JsonPropertyName("list")]
    public IEnumerable<PersonContactPreferencesSnapshot>? PersonContactPreferencesSnapshots { get; init; }

    /// <summary>
    /// Gets the starting change ID.
    /// </summary>
    [JsonPropertyName("fraEndringsId")]
    public long? FromChangeId { get; init; }

    /// <summary>
    /// Gets the ending change ID.
    /// </summary>
    [JsonPropertyName("tilEndringsId")]
    public long? ToChangeId { get; init; }

    /// <summary>
    /// Gets the latest change ID.
    /// </summary>
    [JsonPropertyName("sisteEndringsId")]
    public long? LatestChangeId { get; init; }
}
