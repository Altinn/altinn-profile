#nullable enable

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the results of a user contact details lookup operation.
/// </summary>
public record UserContactDetailsLookupResult
{
    /// <summary>
    /// Gets a list of user contact details that were successfully matched based on the national identity number.
    /// </summary>
    [JsonPropertyName("matchedUserContactDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<UserContactDetails>? MatchedUserContactDetails { get; init; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with user contact details.
    /// </summary>
    [JsonPropertyName("unmatchedNationalIdentityNumbers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactDetailsLookupResult"/> record.
    /// </summary>
    /// <param name="matchedUserContactDetails">The list of contact details that were matched based on the national identity number.</param>
    /// <param name="unmatchedNationalIdentityNumbers">The list of national identity numbers that could not be matched with user contact details.</param>
    public UserContactDetailsLookupResult(
        ImmutableList<UserContactDetails> matchedUserContactDetails,
        ImmutableList<string> unmatchedNationalIdentityNumbers)
    {
        MatchedUserContactDetails = matchedUserContactDetails;
        UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers;
    }
}
