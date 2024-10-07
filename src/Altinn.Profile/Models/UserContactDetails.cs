#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents a user's contact information, including national identity number, contact methods, language preference, and opt-out status.
/// </summary>
public record UserContactDetails
{
    /// <summary>
    /// Gets the national identity number of the user.
    /// </summary>
    [JsonPropertyName("nationalIdentityNumber")]
    public required string NationalIdentityNumber { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user has opted out of being contacted.
    /// </summary>
    [JsonPropertyName("reservation")]
    public bool? Reservation { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the user.
    /// </summary>
    [JsonPropertyName("mobilePhoneNumber")]
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets the language code preferred by the user for communication.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; init; }
}
