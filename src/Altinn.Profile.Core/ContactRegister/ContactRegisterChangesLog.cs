using System.Collections.Immutable;
using System.Text.Json.Serialization;

using Altinn.Profile.Core.Person.ContactPreferences;

namespace Altinn.Profile.Core.ContactRegister;

/// <summary>
/// Represents the changes to a person's contact preferences from the contact register.
/// </summary>
public record ContactRegisterChangesLog
{
    /// <summary>
    /// Gets the collection of snapshots representing the changes to a person's contact preferences.
    /// </summary>
    [JsonPropertyName("list")]
    public IImmutableList<PersonContactPreferencesSnapshot>? ContactPreferencesSnapshots { get; init; }

    /// <summary>
    /// Gets the ending change identifier, which indicates the point at which the system should stop retrieving changes.
    /// </summary>
    [JsonPropertyName("tilEndringsId")]
    public long? EndingIdentifier { get; init; }

    /// <summary>
    /// Gets the most recent change identifier, which represents the last change that was processed by the system.
    /// </summary>
    [JsonPropertyName("sisteEndringsId")]
    public long? LatestChangeIdentifier { get; init; }

    /// <summary>
    /// Gets the starting change identifier indicating the point from which the system begins retrieving changes.
    /// </summary>
    [JsonPropertyName("fraEndringsId")]
    public long? StartingIdentifier { get; init; }
}
