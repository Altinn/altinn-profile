using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the notification status change log of a person.
/// </summary>
public class PersonNotificationStatusChangeLog : IPersonNotificationStatusChangeLog
{
    /// <summary>
    /// Gets the identifier of the person.
    /// </summary>
    [JsonPropertyName("personidentifikator")]
    public string? PersonIdentifier { get; private set; }

    /// <summary>
    /// Gets the reservation details of the person.
    /// </summary>
    [JsonPropertyName("reservasjon")]
    public string? Reservation { get; private set; }

    /// <summary>
    /// Gets the status of the person.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; private set; }

    /// <summary>
    /// Gets the notification status of the person.
    /// </summary>
    [JsonPropertyName("varslingsstatus")]
    public string? NotificationStatus { get; private set; }

    /// <summary>
    /// Gets the contact information change log of the person.
    /// </summary>
    [JsonPropertyName("kontaktinformasjon")]
    public IPersonContactDetailsFromChangeLog? ContactInfoChangeLog { get; private set; }

    /// <summary>
    /// Gets the language preference of the person.
    /// </summary>
    [JsonPropertyName("spraak")]
    public string? Language { get; private set; }

    /// <summary>
    /// Gets the date and time when the person's language preference was updated.
    /// </summary>
    [JsonPropertyName("spraak_oppdatert")]
    public DateTime? LanguageUpdated { get; private set; }
}
