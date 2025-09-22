namespace Altinn.Profile.Integrations.SblBridge.User.NotificationSettings
{
    /// <summary>
    /// Describes an change to notification settings in SBLBridge. 
    /// </summary>
    public class NotificationSettingsChangedRequest
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
        /// Gets or sets the UUID of the added or removed favorite party.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the Email endpoint of the reportee specific to a user
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the Phone number endpoint of the reportee specific to a user
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets notification options chosen for specific services by the user for the reportee
        /// </summary>
        public string[]? ServiceNotificationOptions { get; set; }
    }
}
