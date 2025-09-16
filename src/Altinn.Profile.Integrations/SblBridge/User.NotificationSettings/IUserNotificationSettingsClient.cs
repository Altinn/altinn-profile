namespace Altinn.Profile.Integrations.SblBridge.User.NotificationSettings
{
    /// <summary>
    /// Interface for managing user notificationSettings.
    /// </summary>
    public interface IUserNotificationSettingsClient
    {
        /// <summary>
        /// Updates the user's notificationSettings based on the provided request.
        /// </summary>
        /// <param name="request">The request containing details of the change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateNotificationSettings(NotificationSettingsChangedRequest request);
    }
}
