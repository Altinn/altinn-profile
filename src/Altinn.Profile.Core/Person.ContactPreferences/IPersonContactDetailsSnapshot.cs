namespace Altinn.Profile.Core.Person.ContactPreferences;

/// <summary>
/// Represents a snapshot of the contact details retrieved from the changes log.
/// </summary>
public interface IPersonContactDetailsSnapshot
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the date and time when the email address was last verified.
    /// </summary>
    DateTime? EmailLastVerified { get; }

    /// <summary>
    /// Gets the date and time when the email address was last updated.
    /// </summary>
    DateTime? EmailLastUpdated { get; }

    /// <summary>
    /// Gets a value indicating whether the email address is duplicated.
    /// </summary>
    string? IsEmailDuplicated { get; }

    /// <summary>
    /// Gets a value indicating whether the mobile phone number is duplicated.
    /// </summary>
    string? IsMobileNumberDuplicated { get; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    string? MobileNumber { get; }

    /// <summary>
    /// Gets the date and time when the mobile phone number was last verified.
    /// </summary>
    DateTime? MobileNumberLastVerified { get; }

    /// <summary>
    /// Gets the date and time when the mobile phone number was last updated.
    /// </summary>
    DateTime? MobileNumberLastUpdated { get; }
}
