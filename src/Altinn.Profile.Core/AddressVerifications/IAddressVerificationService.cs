using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// Represents an implementation contract for a business service that can handle address verification, including generating and sending verification codes, and notifying users about address changes via email or SMS.
    /// </summary>
    public interface IAddressVerificationService
    {
        /// <summary>
        /// Generates a verification code, saves it to the database and sends it to the user via email or sms depending on the address type. The code is valid for 15 minutes.
        /// </summary>
        /// <param name="userid">The id of the user</param>
        /// <param name="address">The address to verify</param>
        /// <param name="addressType">The addresstype, sms or email</param>
        /// <param name="languageCode">The language the user has chosen as their preffered language</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns></returns>
        Task GenerateAndSendVerificationCode(int userid, string address, AddressType addressType, string languageCode, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an SMS order to the specified phone number notifying the owner about an address change.
        /// </summary>
        /// <param name="phoneNumber">The phone number to send the SMS to.</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="languageCode">The language code for the SMS content.</param>
        /// <param name="userid">The id of the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NotifySmsAddressChange(string phoneNumber, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an email order to the specified email address notifying the owner about an address change.
        /// </summary>
        /// <param name="emailAddress">The email address to send the email to.</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="languageCode">The language code for the email content.</param>
        /// <param name="userid">The id of the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NotifyEmailAddressChange(string emailAddress, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken);
    }
}
