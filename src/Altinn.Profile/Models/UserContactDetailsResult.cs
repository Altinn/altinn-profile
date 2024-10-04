#nullable enable

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the result of a lookup operation for a user's contact details.
/// It contains information about which contact details were successfully matched and which could not be matched during the lookup process.
/// </summary>
public record UserContactDetailsResult
{
    /// <summary>
    /// Gets the list of user contact details that were successfully matched based on the national identity number.
    /// </summary>
    [JsonPropertyName("matchedContacts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<UserContactDetails>? MatchedContacts { get; init; }

    /// <summary>
    /// Gets the list of user contact details that could not be matched based on the national identity number.
    /// This could be due to an invalid or nonexistent national identity number or other reasons.
    /// </summary>
    [JsonPropertyName("unmatchedContacts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<UserContactDetails>? UnmatchedContacts { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactDetailsResult"/> record.
    /// </summary>
    /// <param name="matchedContacts">The list of contact details that were matched based on the national identity number.</param>
    /// <param name="unmatchedContacts">The list of contact details that were not matched based on the national identity number.</param>
    public UserContactDetailsResult(
        ImmutableList<UserContactDetails> matchedContacts,
        ImmutableList<UserContactDetails> unmatchedContacts)
    {
        MatchedContacts = matchedContacts;
        UnmatchedContacts = unmatchedContacts;
    }
}
