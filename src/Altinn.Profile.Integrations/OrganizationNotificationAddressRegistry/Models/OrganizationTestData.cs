# nullable disable

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// Represents test data for an organization, including contact information such as email and phone.
    /// </summary>
    public class OrganizationTestData
    {
        /// <summary>
        /// Gets or sets the organization number.
        /// </summary>
        public string OrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address of the organization.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the email domain of the organization.
        /// </summary>
        public string EmailDomain { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the organization.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the country code for the organization's phone number.
        /// </summary>
        public string PhoneCountryCode { get; set; }
    }
}
