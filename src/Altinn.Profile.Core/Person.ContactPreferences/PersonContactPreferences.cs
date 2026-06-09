#nullable enable

namespace Altinn.Profile.Core.Person.ContactPreferences;

/// <summary>
/// Represents a person's contact details.
/// </summary>
public record PersonContactPreferences
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets a value indicating whether the person opts out of being contacted.
    /// </summary>
    public bool IsReserved { get; init; }

    /// <summary>
    /// Gets the language code of the person, represented as an ISO 639-1 code.
    /// </summary>
    public string? LanguageCode { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    public string? MobileNumber { get; init; }

    /// <summary>
    /// Gets the national identity number of the person.
    /// </summary>
    public required string NationalIdentityNumber { get; init; }

    /// <summary>
    /// The latest date and time when the mobile number was updated or verified by the user.
    /// </summary>
    public DateTime? MobileNumberLastTouched { get; init; }

    /// <summary>
    /// The latest date and time when the email address was updated or verified by the user.
    /// </summary>
    public DateTime? EmailLastTouched { get; init; }
}
