#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the contact details for a single person, including the national identity number, mobile phone number, email address, language preference, and an opt-out status for being contacted.
/// </summary>
public record PersonContactDetails
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets a value indicating whether the person has opted out of being contacted.
    /// </summary>
    [JsonPropertyName("reservation")]
    public bool? IsReserved { get; init; }

    /// <summary>
    /// Gets the language code preferred by the person for communication.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    [JsonPropertyName("mobilePhoneNumber")]
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the national identity number of the person.
    /// </summary>
    [JsonPropertyName("nationalIdentityNumber")]
    public required string NationalIdentityNumber { get; init; }
}
