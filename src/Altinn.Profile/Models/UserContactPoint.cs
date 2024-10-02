#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents a user contact point.
/// </summary>
public record UserContactPoint
{
    /// <summary>
    /// Gets or sets the national identity number of the user.
    /// </summary>
    [JsonPropertyName("nationalIdentityNumber")]
    public required string NationalIdentityNumber { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user opts out of being contacted.
    /// </summary>
    [JsonPropertyName("reservation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Reservation { get; init; }

    /// <summary>
    /// Gets or sets the mobile phone number of the user.
    /// </summary>
    [JsonPropertyName("mobilePhoneNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [JsonPropertyName("emailAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets or sets the language code of the user.
    /// </summary>
    [JsonPropertyName("languageCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageCode { get; init; }
}
