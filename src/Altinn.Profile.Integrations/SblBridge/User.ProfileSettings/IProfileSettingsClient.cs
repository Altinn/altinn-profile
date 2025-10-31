namespace Altinn.Profile.Integrations.SblBridge.User.ProfileSettings
{
    /// <summary>
    /// Interface for managing the user's portal settings.
    /// </summary>
    public interface IProfileSettingsClient
    {
        /// <summary>
        /// Updates the user's portal settings in A2 based on the provided request.
        /// </summary>
        /// <param name="request">The request containing details of the change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdatePortalSettings(ProfileSettingsChangedRequest request);
    }
}
