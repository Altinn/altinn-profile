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
        public required string RegistryOrganizationNumber { get; set; }

        /// <summary>
        /// The id of the organization in the registry
        /// </summary>
        [Required]
        [Column("registry_organization_id")]
        public required int RegistryOrganizationId { get; set; }

        /// <summary>
        /// A collection of notification addresses associated with this organization
        /// </summary>
        [ForeignKey("fk_registry_organization_id")]
        public List<OrganizationNotificationAddress>? NotificationAddresses { get; set; }
    }
}
