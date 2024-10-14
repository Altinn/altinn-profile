using System;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the contact details of a person from the change log.
/// </summary>
public class PersonContactDetailsFromChangeLog : IPersonContactDetailsFromChangeLog
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    [JsonPropertyName("epostadresse")]
    public string EmailAddress { get; private set; }

    /// <summary>
    /// Gets the date and time when the person's email address was updated.
    /// </summary>
    [JsonPropertyName("epostadresse_oppdatert")]
    public DateTime EmailAddressUpdated { get; private set; }

    /// <summary>
    /// Gets the date and time when the person's email address was last verified.
    /// </summary>
    [JsonPropertyName("epostadresse_sist_verifisert")]
    public DateTime EmailAddressLastVerified { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the person's email address is duplicated.
    /// </summary>
    [JsonPropertyName("epostadresse_duplisert")]
    public string IsEmailAddressDuplicated { get; private set; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer")]
    public string MobilePhoneNumber { get; private set; }

    /// <summary>
    /// Gets the date and time when the person's mobile phone number was updated.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_oppdatert")]
    public DateTime MobilePhoneNumberUpdated { get; private set; }

    /// <summary>
    /// Gets the date and time when the person's mobile phone number was last verified.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_sist_verifisert")]
    public DateTime MobilePhoneNumberLastVerified { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the person's mobile phone number is duplicated.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_duplisert")]
    public string IsMobilePhoneNumberDuplicated { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactDetailsFromChangeLog"/> class.
    /// </summary>
    /// <param name="emailAddress">The email address of the person.</param>
    /// <param name="emailUpdated">The date and time when the email address was updated.</param>
    /// <param name="emailVerified">The date and time when the email address was last verified.</param>
    /// <param name="emailDuplicated">A value indicating whether the email address is duplicated.</param>
    /// <param name="mobileNumber">The mobile phone number of the person.</param>
    /// <param name="mobileUpdated">The date and time when the mobile phone number was updated.</param>
    /// <param name="mobileVerified">The date and time when the mobile phone number was last verified.</param>
    /// <param name="mobileDuplicated">A value indicating whether the mobile phone number is duplicated.</param>
    public PersonContactDetailsFromChangeLog(string emailAddress, DateTime emailUpdated, DateTime emailVerified, string emailDuplicated, string mobileNumber, DateTime mobileUpdated, DateTime mobileVerified, string mobileDuplicated)
    {
        EmailAddress = emailAddress;
        EmailAddressUpdated = emailUpdated;
        EmailAddressLastVerified = emailVerified;
        IsEmailAddressDuplicated = emailDuplicated;

        MobilePhoneNumber = mobileNumber;
        MobilePhoneNumberUpdated = mobileUpdated;
        MobilePhoneNumberLastVerified = mobileVerified;
        IsMobilePhoneNumberDuplicated = mobileDuplicated;
    }
}

/// <summary>
/// Represents the notification status change log of a person.
/// </summary>
public class PersonNotificationStatusChangeLog : IPersonNotificationStatusChangeLog
{
    /// <summary>
    /// Gets the identifier of the person.
    /// </summary>
    [JsonPropertyName("personidentifikator")]
    public string PersonIdentifier { get; private set; }

    /// <summary>
    /// Gets the reservation details of the person.
    /// </summary>
    [JsonPropertyName("reservasjon")]
    public string Reservation { get; private set; }

    /// <summary>
    /// Gets the status of the person.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; private set; }

    /// <summary>
    /// Gets the notification status of the person.
    /// </summary>
    [JsonPropertyName("varslingsstatus")]
    public string NotificationStatus { get; private set; }

    /// <summary>
    /// Gets the contact information change log of the person.
    /// </summary>
    [JsonPropertyName("kontaktinformasjon")]
    public PersonContactDetailsFromChangeLog ContactInfoChangeLog { get; private set; }

    /// <summary>
    /// Gets the language preference of the person.
    /// </summary>
    [JsonPropertyName("spraak")]
    public string Language { get; private set; }

    /// <summary>
    /// Gets the date and time when the person's language preference was updated.
    /// </summary>
    [JsonPropertyName("spraak_oppdatert")]
    public DateTime LanguageUpdated { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactDetailsFromChangeLog"/> class.
    /// </summary>
    /// <param name="personIdentifier">The identifier of the person.</param>
    /// <param name="reservation">The reservation details of the person.</param>
    /// <param name="status">The status of the person.</param>
    /// <param name="notificationStatus">The notification status of the person.</param>
    /// <param name="contactInfoChangeLog">The contact information change log of the person.</param>
    /// <param name="language">The language preference of the person.</param>
    /// <param name="languageUpdated">The date and time when the language preference was updated.</param>
    public PersonNotificationStatusChangeLog(string personIdentifier, string reservation, string status, string notificationStatus, PersonContactDetailsFromChangeLog contactInfoChangeLog, string language, DateTime languageUpdated)
    {
        PersonIdentifier = personIdentifier;
        Reservation = reservation;
        Status = status;
        NotificationStatus = notificationStatus;
        ContactInfoChangeLog = contactInfoChangeLog;
        Language = language;
        LanguageUpdated = languageUpdated;
    }
}
