using System;

namespace Altinn.Profile.Models.Dashboard
{
    /// <summary>
    /// Represents a user's contact point information for the dashboard.
    /// </summary>
    public class DashboardUserContactPointResponse
    {
        /// <summary>
        /// Gets or sets the national identity number of the user
        /// </summary>
        public string? NationalIdentityNumber { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the user has reserved themselves from electronic communication
        /// </summary>
        public bool IsReserved { get; set; }

        /// <summary>
        /// Gets or sets the mobile number
        /// </summary>
        public string? MobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The latest date and time when the mobile number was updated or verified by the user.
        /// </summary>
        public DateTime? MobileNumberLastTouched { get; init; }

        /// <summary>
        /// The latest date and time when the email address was updated or verified by the user.
        /// </summary>
        public DateTime? EmailLastTouched { get; init; }
    }
}
