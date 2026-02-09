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
        /// Tries to verify an address using the provided verification code hash.
        /// </summary>
        /// <param name="addressType">If the address is for sms or email</param>
        /// <param name="address">The address to verify</param>
        /// <param name="userId">The id of the user</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task AddLegacyAddressAsync(AddressType addressType, string address, int userId, CancellationToken cancellationToken);
    }
}
