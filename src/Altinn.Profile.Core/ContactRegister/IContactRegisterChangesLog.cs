using System.Collections.Immutable;

using Altinn.Profile.Core.Person.ContactPreferences;

namespace Altinn.Profile.Core.ContactRegister;

/// <summary>
/// Represents the changes to a person's contact preferences from the contact register.
/// </summary>
public interface IContactRegisterChangesLog
{
    /// <summary>
    /// Gets the collection of snapshots representing the changes to a person's contact preferences.
    /// </summary>
    IImmutableList<PersonContactPreferencesSnapshot>? ContactPreferencesSnapshots { get; }

    /// <summary>
    /// Gets the ending change identifier, which indicates the point at which the system should stop retrieving changes.
    /// </summary>
    long? EndingIdentifier { get; }

    /// <summary>
    /// Gets the most recent change identifier, which represents the last change that was processed by the system.
    /// </summary>
    long? LatestChangeIdentifier { get; }

    /// <summary>
    /// Gets the starting change identifier indicating the point from which the system begins retrieving changes.
    /// </summary>
    long? StartingIdentifier { get; }
}
