using System.Text.Json.Serialization;

using Altinn.Profile.Core.Person.ContactPreferences;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a snapshot of the contact details retrieved from the changes log.
/// </summary>
public class PersonContactDetailsSnapshot : IPersonContactDetailsSnapshot
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    [JsonPropertyName("epostadresse")]
    public string? Email { get; init; }

    /// <summary>
    /// Gets the date and time when the email address was last verified.
    /// </summary>
    [JsonPropertyName("epostadresse_sist_verifisert")]
    public DateTime? EmailLastVerified { get; init; }

    /// <summary>
    /// Gets the date and time when the email address was last updated.
    /// </summary>
    [JsonPropertyName("epostadresse_oppdatert")]
    public DateTime? EmailLastUpdated { get; init; }

    /// <summary>
    /// Gets a value indicating whether the email address is duplicated.
    /// </summary>
    [JsonPropertyName("epostadresse_duplisert")]
    public string? IsEmailDuplicated { get; init; }

    /// <summary>
    /// Gets a value indicating whether the mobile phone number is duplicated.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_duplisert")]
    public string? IsMobileNumberDuplicated { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer")]
    public string? MobileNumber { get; init; }

    /// <summary>
    /// Gets the date and time when the mobile phone number was last verified.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_sist_verifisert")]
    public DateTime? MobileNumberLastVerified { get; init; }

    /// <summary>
    /// Gets the date and time when the mobile phone number was last updated.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_oppdatert")]
    public DateTime? MobileNumberLastUpdated { get; init; }
}
