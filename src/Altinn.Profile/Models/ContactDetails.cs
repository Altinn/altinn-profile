#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the contact information for a single person, including national identity number, contact methods, language preference, and opt-out status.
/// </summary>
public record ContactDetails
{
    /// <summary>
    /// Gets the national identity number of the person.
    /// </summary>
    [JsonPropertyName("nationalIdentityNumber")]
    public required string NationalIdentityNumber { get; init; }

    /// <summary>
    /// Gets a value indicating whether the person has opted out of being contacted.
    /// </summary>
    [JsonPropertyName("reservation")]
    public bool? Reservation { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    [JsonPropertyName("mobilePhoneNumber")]
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets the language code preferred by the person for communication.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; init; }
}
