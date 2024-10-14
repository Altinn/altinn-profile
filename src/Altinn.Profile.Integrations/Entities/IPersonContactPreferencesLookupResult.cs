#nullable enable

using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the result of a lookup operation for contact preferences.
/// </summary>
public interface IPersonContactPreferencesLookupResult
{
    /// <summary>
    /// Gets a list of person contact preferences that were successfully matched during the lookup.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableList{T}"/> of <see cref="IPersonContactPreferences"/> containing the matched person contact preferences.
    /// </value>
    ImmutableList<IPersonContactPreferences>? MatchedPersonContactPreferences { get; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with any person contact preferences.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableList{T}"/> of <see cref="string"/> containing the unmatched national identity numbers.
    /// </value>
    ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; }
}
