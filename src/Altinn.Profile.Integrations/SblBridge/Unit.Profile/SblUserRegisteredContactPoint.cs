namespace Altinn.Profile.Integrations.SblBridge.Unit.Profile
{
    /// <summary>
    /// Model describing the contact information that a user has associated with a party they can represent.
    /// </summary>
    public class SblUserRegisteredContactPoint
    {
        /// <summary>
        /// Gets or sets the legacy user id for the owner of this party notification endpoint.
        /// </summary>
        /// <remarks>
        /// This was named as legacy for consistency. Property for UUID will probably never be added.
        /// </remarks>
        public int LegacyUserId { get; set; }

        /// <summary>
        /// Gets or sets the email address for this contact point.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mobile number for this contact point.
        /// </summary>
        public string MobileNumber { get; set; } = string.Empty;
    }
}
