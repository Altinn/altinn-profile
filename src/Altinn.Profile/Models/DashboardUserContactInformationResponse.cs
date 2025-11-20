#nullable enable

using System;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Response model for user contact information registered for an organization in the Support Dashboard.
    /// Represents a user's personal contact details they have registered for acting on behalf of an organization.
    /// </summary>
    public class DashboardUserContactInformationResponse
    {
        /// <summary>
        /// Gets or sets the national identity number (SSN/D-number) of the user.
        /// </summary>
        public required string NationalIdentityNumber { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the email address registered by the user for this organization.
        /// May be null if no email address is set.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number registered by the user for this organization.
        /// May be null if no phone number is set.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this contact information was last changed.
        /// This timestamp applies to both email and phone number as they are stored together.
        /// </summary>
        public DateTime LastChanged { get; set; }
    }
}
