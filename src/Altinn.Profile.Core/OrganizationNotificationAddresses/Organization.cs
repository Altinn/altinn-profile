namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// class for organizations connection id and orgNumber
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// OrganizationNumber of the organization
        /// </summary>
        public required string RegistryOrganizationNumber { get; set; }

        /// <summary>
        /// The id of the organization in the registry
        /// </summary>
        public required int RegistryOrganizationId { get; set; }

        /// <summary>
        /// A collection of notification addresses associated with this organization
        /// </summary>
        public List<NotificationAddress>? NotificationAddresses { get; set; }
    }
}
