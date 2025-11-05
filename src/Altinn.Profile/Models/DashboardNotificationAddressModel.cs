using System;
using System.ComponentModel.DataAnnotations;

using Altinn.Profile.Validators;

using Microsoft.VisualBasic;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public abstract class DashboardNotificationAddressModel
    {
        /// <summary>
        /// Country code for phone number
        /// </summary>        
        public string CountryCode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>        
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>        
        public string Phone { get; set; }

        /// <summary>
        /// Source organization number
        /// </summary>        
        public string SourceOrgNumber { get; set; }

        /// <summary>
        /// Requested organization number
        /// </summary>        
        public string RequestedOrgNumber { get; set; }

        /// <summary>
        /// Last changed timestamp
        /// </summary>        
        public DateTime? LastChangedTimeStamp { get; set; }
    }
}
