using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// class for organizations connection id and orgNumber
    /// </summary>
    [Table("OfficialContactPoints", Schema = "organization_contact_info")]

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
        /// Gets or sets <see cref="KoFuViID"/>
        /// </summary>
        [Required]
        [StringLength(32)]
        [Column("kofuvi_organization_id")]
        public string KoFuViOrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OfficialContactPoint"/>
        /// </summary>
        public List<OfficialContactPoint> OfficialContactPoints { get; set; }
    }
}
