using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataType = Altinn.Profile.Integrations.SblBridge.Changelog.DataType;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Table of metadata for last changelog sync batch
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
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
        /// What dataType this metadata is for.
        /// </summary>
        [Required]
        public DataType DataType { get; set; }

        /// <summary>
        /// The number of ticks (100 nanoseconds each) representing the last change time.
        /// This is needed because DateTime in C# supports up to 100-nanosecond precision 10^-7
        /// PostgreSQL does not support nanosecond precision, so precision 10^-6
        /// </summary>
        [Required]
        public long LastChangeTicks { get; set; }
    }
}
