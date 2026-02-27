using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Provides methods for sending user-facing notifications related to address changes
    /// and verification codes. This abstraction encapsulates language resolution, message
    /// content building, and notification delivery.
    /// </summary>
    public interface IAltinnUserNotifier
    {
        /// <summary>
        /// Sends a notification to the user informing them that their contact address has been changed.
        /// The user's preferred language is resolved internally.
        /// </summary>
        /// <param name="userId">The ID of the user to notify.</param>
        /// <param name="address">The address (email or phone number) to send the notification to.</param>
        /// <param name="addressType">Whether the address is an email or SMS number.</param>
        /// <param name="partyUuid">The UUID of the party whose settings were changed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NotifyAddressChangeAsync(int userId, string address, AddressType addressType, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a verification code to the user via e-mail or SMS. The user's preferred language
        /// is resolved internally and used to build localized message content.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="address">The address (email or phone number) to send the code to.</param>
        /// <param name="addressType">Whether the address is an email or SMS number.</param>
        /// <param name="verificationCode">The raw (unhashed) verification code to include in the message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendVerificationCodeAsync(int userId, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken);
    }
}
