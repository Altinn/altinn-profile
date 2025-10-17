using System.ComponentModel.DataAnnotations;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// A class describing the query model for contact points for units
    /// </summary>
    public class UnitContactPointLookup
    {
        /// <summary>
        /// Gets or sets the list of organization numbers to lookup contact points for
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> OrganizationNumbers { get; set; } = [];

        /// <summary>
        /// Gets or sets the resource id to filter the contact points by
        /// </summary>
        [Required]
        public string ResourceId { get; set; } = string.Empty;
    }
}
