namespace Altinn.Profile.Core.User.ContactInfo;

/// <summary>
/// A model for creating personal contact information for a non-citizen user
/// </summary>
public record UserContactInfoCreateModel
{
    /// <summary>
    /// The user identifier.
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// UUID of the user
    /// </summary>
    public required Guid UserUuid { get; init; }

    /// <summary>
    /// The Username
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// The email address
    /// </summary>
    public required string EmailAddress { get; init; }

    /// <summary>
    /// The phone number
    /// </summary>
    public string? PhoneNumber { get; init; }
}
