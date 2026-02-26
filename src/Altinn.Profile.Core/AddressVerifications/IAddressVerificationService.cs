using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// Represents an implementation contract for a business service that can handle address verification,
    /// including generating and sending verification codes, and managing verification state.
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
        /// Language resolution and notification delivery are delegated to <see cref="Integrations.IAltinnUserNotifier"/>.
        /// </summary>
        /// <param name="userid">The id of the user</param>
        /// <param name="address">The address to verify</param>
        /// <param name="addressType">The addresstype, sms or email</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GenerateAndSendVerificationCodeAsync(int userid, string address, AddressType addressType, CancellationToken cancellationToken);

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
        /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the submitted code is valid and the address has been successfully verified; otherwise, <c>false</c>.</returns>
        Task<bool> SubmitVerificationCodeAsync(int userid, string address, AddressType addressType, string submittedCode, CancellationToken cancellationToken);

        /// <summary>
        /// Regenerates and sends a verification code for/to the given user and address.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="address">The address to verify</param>
        /// <param name="addressType">The addresstype, sms or email</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if a new verification code was generated and sent to the address; <c>false</c> if the user lacks existing verification codes for that address.</returns>
        Task<bool> ResendVerificationCodeAsync(int userId, string address, AddressType addressType, CancellationToken cancellationToken);
    }
}
