#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents a user's contact information for communication purposes.
/// This includes the user's identity number, contact methods (mobile phone and email),
/// language preference, and a flag indicating whether the user has opted out of contact.
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
    /// A value of <c>true</c> indicates that the user does not wish to receive communications, while <c>false</c> or <c>null</c> indicates that they have not opted out.
    /// </summary>
    [JsonPropertyName("reservation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Reservation { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the user.
    /// </summary>
    [JsonPropertyName("mobilePhoneNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [JsonPropertyName("emailAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets the language code preferred by the user for communication.
    /// </summary>
    [JsonPropertyName("languageCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageCode { get; init; }
}
