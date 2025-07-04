namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Interface for sending notifications such as SMS and email orders.
    /// </summary>
    public interface INotificationsClient
    {
        /// <summary>
        /// Sends an SMS order to the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to send the SMS to.</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="languageCode">The language code for the SMS content.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OrderSms(string phoneNumber, Guid partyUuid, string languageCode, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an email order to the specified email address.
        /// </summary>
        /// <param name="emailAddress">The email address to send the email to.</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="languageCode">The language code for the email content.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OrderEmail(string emailAddress, Guid partyUuid, string languageCode,  CancellationToken cancellationToken);
    }
}
