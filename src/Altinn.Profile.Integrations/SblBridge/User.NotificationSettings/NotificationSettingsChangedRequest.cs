namespace Altinn.Profile.Integrations.SblBridge.User.NotificationSettings
{
    /// <summary>
    /// Describes an change to notification settings in SBLBridge. 
    /// </summary>
    public class NotificationSettingsChangedRequest
    {
        /// <summary>
        /// Gets or sets the type of change. Supported values are "insert" and "delete".
        /// </summary>
        public required string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the date and time for the change.
        /// </summary>
        public DateTime ChangeDateTime { get; set; }

        /// <summary>
        /// Gets or sets the id of the user that made a change to reportee notification settings.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the uuid of the party that the user either added or removed from their reportee notification settings.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the phone number to use for SMS notifications.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address to use for email notifications.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the service options for which the reportee wants to receive notifications.
        /// </summary>  
        public string[]? ServiceOptions { get; set; }

    }
}
