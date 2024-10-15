using Altinn.Profile.Integrations.Entities;

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
                        .Select(d => d.OrderByDescending(p => p.PersonContactDetailsSnapshot?.EmailAddressUpdated)
                                      .ThenByDescending(p => p.PersonContactDetailsSnapshot?.EmailAddressLastVerified)
                                      .ThenByDescending(p => p.PersonContactDetailsSnapshot?.MobilePhoneNumberUpdated)
                                      .ThenByDescending(p => p.PersonContactDetailsSnapshot?.MobilePhoneNumberLastVerified)
                                      .ThenByDescending(p => p.LanguageUpdated)
                                      .First())
                        .ToList();
    }
}
