namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the notification status change log of a person.
/// </summary>
public interface IPersonNotificationStatusChangeLog
{
    /// <summary>
    /// Gets the contact information change log of the person.
    /// </summary>
    IPersonContactDetailsFromChangeLog? ContactInfoChangeLog { get; }

    /// <summary>
    /// Gets the language preference of the person.
    /// </summary>
    string? Language { get; }

    /// <summary>
    /// Gets the date and time when the person's language preference was updated.
    /// </summary>
    DateTime? LanguageUpdated { get; }

    /// <summary>
    /// Gets the notification status of the person.
    /// </summary>
    string? NotificationStatus { get; }

    /// <summary>
    /// Gets the identifier of the person.
    /// </summary>
    string? PersonIdentifier { get; }

    /// <summary>
    /// Gets the reservation details of the person.
    /// </summary>
    string? Reservation { get; }

    /// <summary>
    /// Gets the status of the person.
    /// </summary>
    string? Status { get; }
}
