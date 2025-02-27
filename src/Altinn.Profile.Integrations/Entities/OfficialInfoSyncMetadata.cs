using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Table of metadata for last brreg kof sync batch
    /// </summary>
    [Table("official_info_sync_metadata", Schema = "organization_contact_info")]
    public class OfficialInfoSyncMetadata
    {
        /// <summary>
        /// Gets LastChangedId 
        /// </summary>
        [StringLength(32)]
        [Required]
        [Column("last_changed_id")]
        public string LastChangedId { get; set; }

        /// <summary>
        /// Gets or sets LastChangedDateTime
        /// </summary>
        [Required]
        [Column("last_changed_date_time")]
        public DateTime LastChangedDateTime { get; set; }
    }
}
