using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataType = Altinn.Profile.Integrations.SblBridge.Changelog.DataType;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Table of metadata for last changelog sync batch
    /// </summary>
    [Table("changelog_sync_metadata", Schema = "lease")]
    public class ChangelogSyncMetadata
    {
        /// <summary>
        /// An identifier for this table 
        /// </summary>
        [StringLength(32)]
        [Required]
        public required string LastChangedId { get; set; }

        /// <summary>
        /// The time and date if last sync with changes
        /// </summary>
        [Required]
        public DateTime LastChangedDateTime { get; set; }

        /// <summary>
        /// What dataType this metadata is for.
        /// </summary>
        [Required]
        public DataType DataType { get; set; }
    }
}
