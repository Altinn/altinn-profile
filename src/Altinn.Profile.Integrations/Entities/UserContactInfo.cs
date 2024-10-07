#nullable enable

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a user's contact information.
/// </summary>
public record UserContactInfo : IUserContactInfo
{
    /// <summary>
    /// Gets the national identity number of the user.
    /// </summary>
    /// <remarks>
    /// This is a unique identifier for the user.
    /// </remarks>
    public required string NationalIdentityNumber { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user opts out of being contacted.
    /// </summary>
    /// <remarks>
    /// If <c>true</c>, the user has opted out of being contacted. If <c>false</c>, the user has not opted out.
    /// </remarks>
    public bool? IsReserved { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the user.
    /// </summary>
    /// <remarks>
    /// This is the user's primary contact number.
    /// </remarks>
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    /// <remarks>
    /// This is the user's primary email address.
    /// </remarks>
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets the language code of the user.
    /// </summary>
    /// <remarks>
    /// This is the preferred language of the user, represented as an ISO 639-1 code.
    /// </remarks>
    public string? LanguageCode { get; init; }
}
