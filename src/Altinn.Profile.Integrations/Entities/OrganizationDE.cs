using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Class for organizations connection id and orgNumber
    /// </summary>
    [Table("organizations", Schema = "organization_notification_address")]
    [Index(nameof(RegistryOrganizationNumber), IsUnique = true)]
    public class OrganizationDE
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
        public List<NotificationAddressDE>? NotificationAddresses { get; set; }
    }
}
