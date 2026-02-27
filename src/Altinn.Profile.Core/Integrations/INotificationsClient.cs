namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Interface for sending notifications such as SMS and email orders.
    /// This interface is content-agnostic; callers are responsible for building message content.
    /// </summary>
    public interface INotificationsClient
    {
        /// <summary>
        /// Sends an SMS order to the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to send the SMS to.</param>
        /// <param name="body">The body content of the SMS message.</param>
        /// <param name="sendersReference">A reference string for tracking, typically a party UUID.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OrderSmsAsync(string phoneNumber, string body, string? sendersReference, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an email order to the specified email address.
        /// </summary>
        /// <param name="emailAddress">The email address to send the email to.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email message.</param>
        /// <param name="sendersReference">A reference string for tracking, typically a party UUID.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OrderEmailAsync(string emailAddress, string subject, string body, string? sendersReference, CancellationToken cancellationToken);
    }
}
