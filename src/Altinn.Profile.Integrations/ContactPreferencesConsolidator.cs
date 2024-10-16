using Altinn.Profile.Core.Person.ContactPreferences;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Provides functionality to remove duplicates from a list of person contact preferences snapshots.
/// </summary>
public class ContactPreferencesConsolidator
{
    /// <summary>
    /// Consolidates a list of <see cref="PersonContactPreferencesSnapshot"/> objects by removing duplicates.
    /// </summary>
    /// <param name="snapshots">The list of snapshots to consolidate.</param>
    /// <returns>A consolidated list of <see cref="PersonContactPreferencesSnapshot"/> objects.</returns>
    public static List<PersonContactPreferencesSnapshot> ConsolidateSnapshots(List<PersonContactPreferencesSnapshot> snapshots)
    {
        return snapshots == null
            ? throw new ArgumentNullException(nameof(snapshots))
            : snapshots.GroupBy(e => e.PersonIdentifier)
                        .Select(d => d.OrderByDescending(p => p.ContactDetailsSnapshot?.EmailLastUpdated)
                                      .ThenByDescending(p => p.ContactDetailsSnapshot?.EmailLastVerified)
                                      .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastUpdated)
                                      .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastVerified)
                                      .ThenByDescending(p => p.LanguageLastUpdated)
                                      .First())
                        .ToList();
    }
}
