#nullable enable

using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Defines the result of a user contact information lookup.
/// </summary>
public interface IUserContactResult
{
    /// <summary>
    /// Gets a list of user contact information that was successfully matched during the lookup.
    /// </summary>
    ImmutableList<IUserContact>? MatchedUserContact { get; init; }

    /// <summary>
    /// Gets a list of user contact information that was not matched during the lookup.
    /// </summary>
    ImmutableList<IUserContact>? UnmatchedUserContact { get; init; }
}
