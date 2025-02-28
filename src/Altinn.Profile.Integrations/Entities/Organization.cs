using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// class for organizations connection id and orgNumber
    /// </summary>
    [Table("Organizations", Schema = "organization_contact_info")]

    public class Organization
    {
        /// <summary>
        /// Gets or sets <see cref="KoFuViOrganizationNumber"/>
        /// </summary>
        [Required]
        [StringLength(9)]
        [Column("kofuvi_organization_number")]
        public int KoFuViOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets <see cref="KoFuViOrganizationId"/>
        /// </summary>
        [Required]
        [StringLength(32)]
        [Column("kofuvi_organization_id")]
        public required string KoFuViOrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OfficialContactPoint"/>
        /// </summary>
        public List<OfficialContactPoint>? OfficialContactPoints { get; set; }
    }
}
