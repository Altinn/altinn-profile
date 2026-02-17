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
        /// <returns>A Task containing the <see cref="VerificationType"/>. If neither verified nor any verification codes exists, the address will be considered Legacy.</returns>
        Task<VerificationType> GetVerificationStatusAsync(int userId, AddressType addressType, string address, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the verified addresses for a given user.
        /// </summary>
        /// <param name="userId">The ID of the user whose verified addresses are to be retrieved.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of verified addresses.</returns>
        Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the verification code for a given address.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="addressType">If the address is for sms or email</param>
        /// <param name="address">The address to check</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task<VerificationCode?> GetVerificationCodeAsync(int userId, AddressType addressType, string address, CancellationToken cancellationToken);

        /// <summary>
        /// Increments the number of failed verification attempts for a given verification code.
        /// </summary>
        /// <param name="verificationCode">The verification code that should increase number of failed attempts </param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task IncrementFailedAttemptsAsync(VerificationCode verificationCode);

        /// <summary>
        /// Complete the address verification.
        /// </summary>
        /// <param name="verificationCode">The verification code that is validated</param>
        /// <param name="addressType">If the address is for sms or email</param>
        /// <param name="address">The address to verify</param>
        /// <param name="userId">The id of the user</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task CompleteAddressVerificationAsync(VerificationCode verificationCode, AddressType addressType, string address, int userId);
    }
}
