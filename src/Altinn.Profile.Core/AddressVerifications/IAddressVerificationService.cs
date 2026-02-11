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
    }
}
