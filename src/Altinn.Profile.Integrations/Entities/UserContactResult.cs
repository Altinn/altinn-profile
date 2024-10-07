#nullable enable

using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the result of a user contact information lookup, containing matched and unmatched entries.
/// </summary>
public record UserContactResult : IUserContactResult
{
    /// <summary>
    /// Gets a list of user contact information that was successfully matched during the lookup.
    /// </summary>
    public ImmutableList<IUserContact>? MatchedUserContact { get; init; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with user contact details.
    /// </summary>
    public ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; init; }
}
