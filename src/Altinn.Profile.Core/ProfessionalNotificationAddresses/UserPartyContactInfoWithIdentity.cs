namespace Altinn.Profile.Core.ProfessionalNotificationAddresses;

/// <summary>
/// Extended data model for user party contact info that includes user identity information (SSN and Name).
/// Used by Dashboard endpoints to display contact information with user identity.
/// This also includes the organization number the user is acting on behalf of.
/// </summary>
public class UserPartyContactInfoWithIdentity
{
    /// <summary>
    /// The national identity number (SSN/D-number) of the user
    /// </summary>
    public string? NationalIdentityNumber { get; set; }

    /// <summary>
    /// The name of the user
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The email address. May be null if no email address is set.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// The phone number. May be null if no phone number is set.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The organization number the user is acting on behalf of.
    /// May be null if no organization number is set.
    /// </summary>
    public string? OrganizationNumber { get; set; }

    /// <summary>
    /// Date of last change (UTC)
    /// </summary>
    public DateTime LastChanged { get; set; }
}
