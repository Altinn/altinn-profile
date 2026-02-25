using System.ComponentModel.DataAnnotations;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// GroupRequest to create or update a group of parties
    /// </summary>
    public class GroupRequest
    {
        /// <summary>
        /// The name of the group
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Name { get; set; }
    }
}
