#nullable enable

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the result of a contact details lookup operation for one or more persons.
/// </summary>
public record PersonContactDetailsLookupResult
{
    /// <summary>
    /// Gets a list of person contact details that were successfully matched based on the national identity number.
    /// </summary>
    [JsonPropertyName("matchedPersonContactDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<PersonContactDetails>? MatchedPersonContactDetails { get; init; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with any contact details.
    /// </summary>
    [JsonPropertyName("unmatchedNationalIdentityNumbers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactDetailsLookupResult"/> record.
    /// </summary>
    /// <param name="matchedPersonContactDetails">The list of person contact details that were successfully matched based on the national identity number.</param>
    /// <param name="unmatchedNationalIdentityNumbers">The list of national identity numbers that could not be matched with any contact details.</param>
    public PersonContactDetailsLookupResult(
        ImmutableList<PersonContactDetails> matchedPersonContactDetails,
        ImmutableList<string> unmatchedNationalIdentityNumbers)
    {
        MatchedPersonContactDetails = matchedPersonContactDetails;
        UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers;
    }
}
