using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a log of changes to a person's contact preferences, including contact information, language preference, notification status, and other details.
/// </summary>
public class PersonContactPreferencesSnapshot : IPersonContactPreferencesSnapshot
{
    /// <summary>
    /// Gets the contact information details of the person.
    /// </summary>
    [JsonPropertyName("kontaktinformasjon")]
    public PersonContactDetailsSnapshot? PersonContactDetailsSnapshot { get; init; }

    /// <summary>
    /// Gets the language preference of the person.
    /// </summary>
    [JsonPropertyName("spraak")]
    public string? Language { get; init; }

    /// <summary>
    /// Gets the date and time when the person's language preference was last updated.
    /// </summary>
    [JsonPropertyName("spraak_oppdatert")]
    public DateTime? LanguageUpdated { get; init; }

    /// <summary>
    /// Gets the notification status of the person.
    /// </summary>
    [JsonPropertyName("varslingsstatus")]
    public string? NotificationStatus { get; init; }

    /// <summary>
    /// Gets the unique identifier of the person.
    /// </summary>
    [JsonPropertyName("personidentifikator")]
    public string? PersonIdentifier { get; init; }

    /// <summary>
    /// Gets the reservation details of the person.
    /// </summary>
    [JsonPropertyName("reservasjon")]
    public string? Reservation { get; init; }

    /// <summary>
    /// Gets the current status of the person.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
