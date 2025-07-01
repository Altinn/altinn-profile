namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Configuration object used to hold settings for all Altinn Notifications integrations.
    /// </summary>
    public class NotificationsSettings
    {
        /// <summary>
        /// Gets or sets the url for the Notifications API
        /// </summary>
        public string ApiNotificationsEndpoint { get; set; } = string.Empty;
    }
}
