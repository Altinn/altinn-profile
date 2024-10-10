#nullable enable

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a person's contact details.
/// </summary>
public interface IPersonContactDetails
{
    /// <summary>
    /// Gets the national identity number of the person.
    /// </summary>
    string NationalIdentityNumber { get; }

    /// <summary>
    /// Gets a value indicating whether the person opts out of being contacted.
    /// </summary>
    bool? IsReserved { get; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    string? MobilePhoneNumber { get; }

    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    string? EmailAddress { get; }

    /// <summary>
    /// Gets the language code of the person, represented as an ISO 639-1 code.
    /// </summary>
    string? LanguageCode { get; }
}
