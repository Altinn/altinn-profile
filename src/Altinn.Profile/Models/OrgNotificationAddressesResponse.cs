using System.Collections.Generic;

using Altinn.Profile.Core.OrganizationNotificationAddresses;

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

    /// <summary>
    /// Creates a new instance of <see cref="OrgNotificationAddressesResponse"/> from a list of <see cref="Organization"/>.
    /// </summary>
    /// <param name="organizations">A list of organizations to create the response from.</param>
    /// <returns>A new instance of <see cref="OrgNotificationAddressesResponse"/> containing the notification addresses for the provided organizations.</returns>
    public static OrgNotificationAddressesResponse Create(IEnumerable<Organization> organizations)
    {
        var orgContacts = new OrgNotificationAddressesResponse();
        foreach (var organization in organizations)
        {
            var contactPoints = new NotificationAddresses
            {
                OrganizationNumber = organization.OrganizationNumber,
                AddressOrigin = organization.AddressOrigin,
            };

            if (organization.NotificationAddresses?.Count > 0)
            {
                foreach (var notificationAddress in organization.NotificationAddresses)
                {
                    if (notificationAddress.IsSoftDeleted == true || notificationAddress.HasRegistryAccepted == false)
                    {
                        continue;
                    }

                    switch (notificationAddress.AddressType)
                    {
                        case AddressType.Email:
                            contactPoints.EmailList.Add(notificationAddress.FullAddress);
                            break;
                        case AddressType.SMS:
                            contactPoints.MobileNumberList.Add(notificationAddress.FullAddress);
                            break;
                    }
                }
            }

            orgContacts.ContactPointsList.Add(contactPoints);
        }

        return orgContacts;
    }
}
