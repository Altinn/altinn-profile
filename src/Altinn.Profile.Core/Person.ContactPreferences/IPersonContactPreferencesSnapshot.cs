namespace Altinn.Profile.Core.Person.ContactPreferences;

/// <summary>
/// Represents a snapshot of a person's contact preferences, including contact information, language preference, notification status, and other details.
/// </summary>
public interface IPersonContactPreferencesSnapshot
{
    /// <summary>
    /// Gets the contact information details of the person.
    /// </summary>
    PersonContactDetailsSnapshot? ContactDetailsSnapshot { get; }

    /// <summary>
    /// Gets the language preference of the person.
    /// </summary>
    string? Language { get; }

    /// <summary>
    /// Gets the date and time when the person's language preference was last updated.
    /// </summary>
    DateTime? LanguageLastUpdated { get; }

    /// <summary>
    /// Gets the notification status of the person.
    /// </summary>
    string? NotificationStatus { get; }

    /// <summary>
    /// Gets the unique identifier of the person.
    /// </summary>
    string? PersonIdentifier { get; }

    /// <summary>
    /// Gets the reservation status of the person.
    /// </summary>
    string? Reservation { get; }

    /// <summary>
    /// Gets the current status of the person.
    /// </summary>
    string? Status { get; }
}
