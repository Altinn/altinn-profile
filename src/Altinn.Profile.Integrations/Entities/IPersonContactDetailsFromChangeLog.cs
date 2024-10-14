namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the contact details of a person from the changes log.
/// </summary>
public interface IPersonContactDetailsFromChangeLog
{
    /// <summary>
    /// Gets the email address.
    /// </summary>
    string? EmailAddress { get; }

    /// <summary>
    /// Gets the date and time when the email address was last verified.
    /// </summary>
    DateTime? EmailAddressLastVerified { get; }

    /// <summary>
    /// Gets the date and time when the email address was updated.
    /// </summary>
    DateTime? EmailAddressUpdated { get; }

    /// <summary>
    /// Gets a value indicating whether the email address is duplicated.
    /// </summary>
    string? IsEmailAddressDuplicated { get; }

    /// <summary>
    /// Gets a value indicating whether the mobile phone number is duplicated.
    /// </summary>
    string? IsMobilePhoneNumberDuplicated { get; }

    /// <summary>
    /// Gets the mobile phone number.
    /// </summary>
    string? MobilePhoneNumber { get; }

    /// <summary>
    /// Gets the date and time when the mobile phone number was last verified.
    /// </summary>
    DateTime? MobilePhoneNumberLastVerified { get; }

    /// <summary>
    /// Gets the date and time when the mobile phone number was updated.
    /// </summary>
    DateTime? MobilePhoneNumberUpdated { get; }
}
