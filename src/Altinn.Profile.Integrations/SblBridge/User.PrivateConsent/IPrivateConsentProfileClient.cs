namespace Altinn.Profile.Integrations.SblBridge.User.PrivateConsent
{
    /// <summary>
    /// Interface for managing the user's private consent.
    /// </summary>
    public interface IPrivateConsentProfileClient
    {
        /// <summary>
        /// Updates the user's private consent profile in A2 based on the provided request.
        /// </summary>
        /// <param name="request">The request containing details of the change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdatePrivateConsent(PrivateConsentChangedRequest request);
    }
}
