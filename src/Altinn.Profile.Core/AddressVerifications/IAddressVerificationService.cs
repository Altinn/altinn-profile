using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// Represents an implementation contract for a business service that can handle address verification, including generating and sending verification codes, and notifying users about address changes via email or SMS.
    /// </summary>
    public interface IAddressVerificationService
    {
        /// <summary>
        /// Gets the verified addresses for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user whose verified addresses are to be retrieved.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of verified addresses.</returns>
        Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Generates a verification code, saves it to the database and sends it to the user via email or sms depending on the address type. The code is valid for 15 minutes.
        /// </summary>
        /// <param name="userid">The id of the user</param>
        /// <param name="address">The address to verify</param>
        /// <param name="addressType">The addresstype, sms or email</param>
        /// <param name="verificationCode">The verification code provided by the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns></returns>
        Task<bool> SubmitVerificationCodeAsync(int userid, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken);
    }
}
