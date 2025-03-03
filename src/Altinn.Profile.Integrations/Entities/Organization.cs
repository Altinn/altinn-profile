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
        /// Gets or sets <see cref="RegistryOrganizationNumber"/>
        /// </summary>
        [Required]
        [StringLength(9)]
        [Column("registry_organization_number")]
        public int RegistryOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets <see cref="RegistryOrganizationId"/>
        /// </summary>
        [Required]
        [StringLength(32)]
        [Column("registry_organization_id")]
        public required string RegistryOrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OrganizationNotificationAddress"/>
        /// </summary>
        public List<OrganizationNotificationAddress>? NotificationAddresses { get; set; }
    }
}
