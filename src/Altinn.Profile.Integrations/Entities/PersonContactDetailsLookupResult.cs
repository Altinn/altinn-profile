#nullable enable

using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the result of a lookup operation for contact details.
/// </summary>
public record PersonContactDetailsLookupResult : IPersonContactDetailsLookupResult
{
    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with any person contact details.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableList{T}"/> of <see cref="string"/> containing the unmatched national identity numbers.
    /// </value>
    public ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; init; }

    /// <summary>
    /// Gets a list of person contact details that were successfully matched during the lookup.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableList{T}"/> of <see cref="IPersonContactDetails"/> containing the matched person contact details.
    /// </value>
    public ImmutableList<IPersonContactDetails>? MatchedPersonContactDetails { get; init; }
}
