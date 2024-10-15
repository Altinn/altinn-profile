namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a log of changes to a person's contact preferences.
/// </summary>
public interface IPersonContactPreferencesChangesLog
{
    /// <summary>
    /// Gets the list of snapshots representing the changes to the person's contact preferences.
    /// </summary>
    /// <value>A collection of <see cref="PersonContactPreferencesSnapshot"/> objects.</value>
    IEnumerable<PersonContactPreferencesSnapshot>? PersonContactPreferencesSnapshots { get; }

    /// <summary>
    /// Gets the starting change ID.
    /// </summary>
    long? FromChangeId { get; }

    /// <summary>
    /// Gets the ending change ID.
    /// </summary>
    long? ToChangeId { get; }

    /// <summary>
    /// Gets the latest change ID.
    /// </summary>
    long? LatestChangeId { get; }
}
