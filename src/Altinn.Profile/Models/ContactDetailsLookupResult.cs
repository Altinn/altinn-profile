#nullable enable

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the results of a contact details lookup operation.
/// </summary>
public record ContactDetailsLookupResult
{
    /// <summary>
    /// Gets a list of contact details that were successfully matched based on the national identity number.
    /// </summary>
    [JsonPropertyName("matchedContactDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<ContactDetails>? MatchedContactDetails { get; init; }

    /// <summary>
    /// Gets a list of national identity numbers that could not be matched with any contact details.
    /// </summary>
    [JsonPropertyName("unmatchedNationalIdentityNumbers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImmutableList<string>? UnmatchedNationalIdentityNumbers { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactDetailsLookupResult"/> record.
    /// </summary>
    /// <param name="matchedContactDetails">The list of contact details that were successfully matched based on the national identity number.</param>
    /// <param name="unmatchedNationalIdentityNumbers">The list of national identity numbers that could not be matched with any contact details.</param>
    public ContactDetailsLookupResult(
        ImmutableList<ContactDetails> matchedContactDetails,
        ImmutableList<string> unmatchedNationalIdentityNumbers)
    {
        MatchedContactDetails = matchedContactDetails;
        UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers;
    }
}
