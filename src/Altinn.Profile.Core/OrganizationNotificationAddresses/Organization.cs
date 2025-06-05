namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Represents an organization with associated notification addresses
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// OrganizationNumber of the organization
        /// </summary>
        public required string OrganizationNumber { get; set; }

        /// <summary>
        /// OrganizationNumber of the organization where the address was found
        /// </summary>
        public string? AddressOrigin { get; set; }

        /// <summary>
        /// A collection of notification addresses associated with this organization
        /// </summary>
        public List<NotificationAddress>? NotificationAddresses { get; set; }

        /// <summary>
        /// OrganizationNumber of the organization where the address was found
        /// </summary>
        public string GetAddressOrigin() => AddressOrigin ?? OrganizationNumber;
    }
}
