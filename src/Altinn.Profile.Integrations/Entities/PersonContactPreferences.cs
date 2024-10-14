#nullable enable

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a person's contact details.
/// </summary>
public record PersonContactPreferences : IPersonContactPreferences
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets a value indicating whether the person opts out of being contacted.
    /// </summary>
    public bool? IsReserved { get; init; }

    /// <summary>
    /// Gets the language code of the person, represented as an ISO 639-1 code.
    /// </summary>
    public string? LanguageCode { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the national identity number of the person.
    /// </summary>
    public required string NationalIdentityNumber { get; init; }
}
