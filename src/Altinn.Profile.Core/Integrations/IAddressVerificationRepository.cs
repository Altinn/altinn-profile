using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Represents an implementation contract for a repository that can handle address verification.
    /// </summary>
    public interface IAddressVerificationRepository
    {
        /// <summary>
        /// Adds a new verification code to the database.
        /// </summary>
        /// <param name="verificationCode">The verification code to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task AddNewVerificationCodeAsync(VerificationCode verificationCode);

        /// <summary>
        /// Retrieves the verification status for an address.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="addressType">If the address is for sms or email</param>
        /// <param name="address">The address to check</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A Task containing the <see cref="VerificationType"/> or null if the address has not been verified.</returns>
        Task<VerificationType?> GetVerificationStatusAsync(int userId, AddressType addressType, string address, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the verified addresses for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user whose verified addresses are to be retrieved.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of verified addresses.</returns>
        Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken);
    }
}
