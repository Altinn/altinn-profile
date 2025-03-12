using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// class for organizations connection id and orgNumber
    /// </summary>
    [Table("organizations", Schema = "organization_notification_address")]

    public class Organization
    {
        /// <summary>
        /// OrganizationNumber of the organization
        /// </summary>
        [Required]
        [StringLength(9)]
        public required string RegistryOrganizationNumber { get; set; }

        /// <summary>
        /// The incremental id of the organization in the database
        /// </summary>
        [Required]
        public int RegistryOrganizationId { get; set; }

        /// <summary>
        /// A collection of notification addresses associated with this organization
        /// </summary>
        [InverseProperty("Organization")]
        public List<OrganizationNotificationAddress>? NotificationAddresses { get; set; }
    }
}
