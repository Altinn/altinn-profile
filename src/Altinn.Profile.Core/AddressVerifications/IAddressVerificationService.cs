using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// Represents an implementation contract for a business service that can handle address verification, including generating and sending verification codes, and notifying users about address changes via email or SMS.
    /// </summary>
    public interface IAddressVerificationService
    {
        /// <summary>
        /// Get verification status for the specified email address and phone number. If the email address or phone number is null or empty, the corresponding verification status will be returned as null.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="emailAddress">The email address to check the verification status for</param>
        /// <param name="phoneNumber">The phone number to check the verification status for</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task<(VerificationType? EmailVerificationStatus, VerificationType? SmsVerificationStatus)> GetVerificationStatusAsync(int userId, string? emailAddress, string? phoneNumber, CancellationToken cancellationToken);

        /// <summary>
        /// Generates a verification code, saves it to the database and sends it to the user via email or sms depending on the address type. The code is valid for 15 minutes.
        /// </summary>
        /// <param name="userid">The id of the user</param>
        /// <param name="address">The address to verify</param>
        /// <param name="addressType">The addresstype, sms or email</param>
        /// <param name="languageCode">The language the user has chosen as their prefered language</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns></returns>
        Task GenerateAndSendVerificationCodeAsync(int userid, string address, AddressType addressType, string languageCode, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an SMS order to the specified phone number notifying the owner about an address change.
        /// </summary>
        /// <param name="phoneNumber">The phone number to send the SMS to.</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="languageCode">The language code for the SMS content.</param>
        /// <param name="userid">The id of the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NotifySmsAddressChangeAsync(string phoneNumber, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an email order to the specified email address notifying the owner about an address change.
        /// </summary>
        /// <param name="emailAddress">The email address to send the email to.</param>
        /// <param name="partyUuid">The partyUuid for the party the address was changed for</param>
        /// <param name="languageCode">The language code for the email content.</param>
        /// <param name="userid">The id of the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NotifyEmailAddressChangeAsync(string emailAddress, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the verified addresses for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user whose verified addresses are to be retrieved.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of verified addresses.</returns>
        Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Submits a verification code for the given user and address. If the code is valid, the verification process completes, and the user's address becomes verified.
        /// </summary>
        /// <param name="userid">The id of the user</param>
        /// <param name="address">The address to verify</param>
        /// <param name="addressType">The addresstype, sms or email</param>
        /// <param name="submittedCode">The verification code provided by the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns></returns>
        Task<bool> SubmitVerificationCodeAsync(int userid, string address, AddressType addressType, string submittedCode, CancellationToken cancellationToken);
    }
}
