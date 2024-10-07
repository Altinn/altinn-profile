#nullable enable

using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Defines the result of a user contact information lookup.
/// </summary>
public interface IUserContactInfoLookupResult
{
    /// <summary>
    /// Gets a list of user contact information that was successfully matched during the lookup.
    /// </summary>
    ImmutableList<IUserContactInfo>? MatchedUserContact { get; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with any user contact details.
    /// </summary>
    ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; }
}
