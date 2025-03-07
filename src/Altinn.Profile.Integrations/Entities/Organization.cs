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
        [Column("registry_organization_number")]
        public int RegistryOrganizationNumber { get; set; }

        /// <summary>
        /// The id of the organization in the registry
        /// </summary>
        [Required]
        [StringLength(32)]
        [Column("registry_organization_id")]
        public required string RegistryOrganizationId { get; set; }

        /// <summary>
        /// A collection of notification addresses associated with this organization
        /// </summary>
        public List<OrganizationNotificationAddress>? NotificationAddresses { get; set; }
    }
}
