namespace Altinn.Profile.Core.ProfessionalNotificationAddresses;

/// <summary>
/// Extended data model for user party contact info that includes user identity information (SSN and Name).
/// Used by Dashboard endpoints to display contact information with user identity.
/// This also includes the organization number the user is acting on behalf of.
/// </summary>
/// <remarks>
/// Create UserPartyContactInfoWithIdentity from UserPartyContactInfo and UserProfile
/// </remarks>
/// <param name="userPartyContactInfo">The user party contact info</param>
/// <param name="name">The name of the user</param>
/// <param name="ssn">The national identity number (SSN/D-number) of the user</param>
/// <param name="organizationNumber">The organization number the user is acting on behalf of</param>
public class UserPartyContactInfoWithIdentity(UserPartyContactInfo userPartyContactInfo, string name, string? ssn, string? organizationNumber)
{
    /// <summary>
    /// The national identity number (SSN/D-number) of the user
    /// </summary>
    public string? NationalIdentityNumber { get; set; } = ssn;

    /// <summary>
    /// The name of the user
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// The email address. May be null if no email address is set.
    /// </summary>
    public string? EmailAddress { get; set; } = userPartyContactInfo.EmailAddress;

    /// <summary>
    /// The phone number. May be null if no phone number is set.
    /// </summary>
    public string? PhoneNumber { get; set; } = userPartyContactInfo.PhoneNumber;

    /// <summary>
    /// The organization number the user is acting on behalf of.
    /// May be null if no organization number is set.
    /// </summary>
    public string? OrganizationNumber { get; set; } = organizationNumber;

    /// <summary>
    /// Date of last change (UTC)
    /// </summary>
    public DateTime LastChanged { get; set; } = userPartyContactInfo.LastChanged;

    /// <summary>
    /// List of resources to get notifications for. This is a list of resource IDs with the prefix "urn:altinn:resource:".
    /// </summary>
    public List<string>? ResourceIncludeList { get; set; } = userPartyContactInfo.GetResourceIncludeList();
}
