using System;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// GroupResponse is used to represent a group of parties
    /// </summary>
    public class GroupResponse
    {
        /// <summary>
        /// The unique identifier of the group
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// The name of the group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A flag indicating whether the group is a group of favorite parties
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Array of party IDs that belong to this group
        /// </summary>
        public Guid[] Parties { get; set; }
    }
}
