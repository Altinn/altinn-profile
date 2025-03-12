using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Table of metadata for last brreg kof sync batch
    /// </summary>
    [Table("registry_sync_metadata", Schema = "organization_notification_address")]
    public class RegistrySyncMetadata
    {
        /// <summary>
        /// An identifier for this table 
        /// </summary>
        [StringLength(32)]
        [Required]
        public string? LastChangedId { get; set; }

        /// <summary>
        /// The time and date if last sync with changes
        /// </summary>
        [Required]
        public DateTime LastChangedDateTime { get; set; }
    }
}
