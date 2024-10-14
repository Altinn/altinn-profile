using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the notification status change log of a person.
/// </summary>
public class PersonChangeLog : IPersonChangeLog
{
    /// <summary>
    /// Gets the identifier of the person.
    /// </summary>
    [JsonPropertyName("personidentifikator")]
    public string? PersonIdentifier { get; init; }

    /// <summary>
    /// Gets the reservation details of the person.
    /// </summary>
    [JsonPropertyName("reservasjon")]
    public string? Reservation { get; init; }

    /// <summary>
    /// Gets the status of the person.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Gets the notification status of the person.
    /// </summary>
    [JsonPropertyName("varslingsstatus")]
    public string? NotificationStatus { get; init; }

    /// <summary>
    /// Gets the contact information change log of the person.
    /// </summary>
    [JsonPropertyName("kontaktinformasjon")]
    public PersonContactDetailsSnapshot? ContactInfoChangeLog { get; init; }

    /// <summary>
    /// Gets the language preference of the person.
    /// </summary>
    [JsonPropertyName("spraak")]
    public string? Language { get; init; }

    /// <summary>
    /// Gets the date and time when the person's language preference was updated.
    /// </summary>
    [JsonPropertyName("spraak_oppdatert")]
    public DateTime? LanguageUpdated { get; init; }
}
