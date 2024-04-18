using System.Collections.Generic;

namespace Altinn.Profile.Core.User.ContactPoints
{
    /// <summary>
    /// Class describing the contact points of a user
    /// </summary>
    public class UserContactPointAvailability
    {
        /// <summary>
        /// Gets or sets the ID of the user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the national identityt number of the user
        /// </summary>
        public string NationalIdentityNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a boolean indicating whether the user has reserved themselves from electronic communication
        /// </summary>
        public bool IsReserved { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the user has registered a mobile number
        /// </summary>
        public bool MobileNumberRegistered { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the user has registered an email address
        /// </summary>
        public bool EmailRegistered { get; set; }
    }

    /// <summary>
    /// A list representation of <see cref="UserContactPointAvailability"/>
    /// </summary>
    public class UserContactPointAvailabilityList
    {
        /// <summary>
        /// A list containing contact point availabiliy for users
        /// </summary>
        public List<UserContactPointAvailability> AvailabilityList { get; set; } = [];
    }
}
