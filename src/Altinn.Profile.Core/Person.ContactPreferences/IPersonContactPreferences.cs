namespace Altinn.Profile.Core.Person.ContactPreferences;

/// <summary>
/// Defines the contact preferences for a person, including their national identity number,
/// contact opt-out status, mobile phone number, email address, and preferred language.
/// </summary>
public interface IPersonContactPreferences
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the person opts out of being contacted.
    /// </summary>
    bool? IsReserved { get; }

    /// <summary>
    /// Gets the language code of the person, represented as an ISO 639-1 code.
    /// </summary>
    string? LanguageCode { get; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    string? MobileNumber { get; }

    /// <summary>
    /// Gets the national identity number of the person.
    /// </summary>
    string NationalIdentityNumber { get; }
}
