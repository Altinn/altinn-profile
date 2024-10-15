#nullable enable

using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the result of a lookup operation for contact preferences.
/// </summary>
public record PersonContactPreferencesLookupResult : IPersonContactPreferencesLookupResult
{
    /// <summary>
    /// Gets a list of person contact preferences that were successfully matched during the lookup.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableList{T}"/> of <see cref="IPersonContactPreferences"/> containing the matched person contact preferences.
    /// </value>
    public ImmutableList<PersonContactPreferences>? MatchedPersonContactPreferences { get; init; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with any person contact preferences.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableList{T}"/> of <see cref="string"/> containing the unmatched national identity numbers.
    /// </value>
    public ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; init; }
}
