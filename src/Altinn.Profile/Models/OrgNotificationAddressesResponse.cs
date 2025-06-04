using System.Collections.Generic;

namespace Altinn.Profile.Models;

/// <summary>
/// A list representation of <see cref="NotificationAddresses"/>
/// </summary>
public class OrgNotificationAddressesResponse
{
    /// <summary>
    /// A list containing notification addresses for organizations
    /// </summary>
    public List<NotificationAddresses> ContactPointsList { get; set; } = [];

    /// <summary>
    /// Class describing the notification addresses for an organization as notifications services uses
    /// </summary>
    public class NotificationAddresses
    {
        /// <summary>
        /// Gets or sets the organization number for the organization
        /// </summary>
        public string OrganizationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the organization number for the organization where the address originated
        /// </summary>
        /// <remarks>This will be the same as <see cref="OrganizationNumber"/> if the address is from this unit, otherwise it will be from a parent unit</remarks>
        public string AddressOrigin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the party id of the organization
        /// </summary>
        public int PartyId { get; set; }

        /// <summary>
        /// Gets or sets a list of official mobile numbers
        /// </summary>
        public List<string> MobileNumberList { get; set; } = [];

        /// <summary>
        /// Gets or sets a list of official email addresses
        /// </summary>
        public List<string> EmailList { get; set; } = [];
    }
}
