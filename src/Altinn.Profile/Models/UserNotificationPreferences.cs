#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents a user's notification preferences.
/// </summary>
public record UserNotificationPreferences
{
    /// <summary>
    /// Gets the national identity number of the user.
    /// </summary>
    [JsonPropertyName("nationalIdentityNumbers")]
    public required string NationalIdentityNumber { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user opts out of being contacted.
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
    /// Gets the language code of the user.
    /// </summary>
    [JsonPropertyName("languageCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageCode { get; init; }
}
