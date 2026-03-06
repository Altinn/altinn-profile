namespace Altinn.Profile.Integrations.SblBridge.User.PrivateConsent
{
    /// <summary>
    /// Describes an event where a user made some change to their portal settings preferences.
    /// </summary>
    public class PrivateConsentChangedRequest
    {
        /// <summary>
        /// Gets or sets the type of change. Supported values are "insert", "update" and "delete".
        /// </summary>
        public required string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the date and time for the change.
        /// </summary>
        public DateTime ChangeDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the language the user has selected in Altinn portal.
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user should not be prompted for party selection.
        /// Can be set without using PreselectedPartyUuid.
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}
