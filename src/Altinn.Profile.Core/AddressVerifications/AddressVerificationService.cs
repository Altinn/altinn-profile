using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// A service for handling address verification processes, including generating and sending verification codes via email or SMS.
    /// </summary>
    public class AddressVerificationService(IAddressVerificationRepository addressVerificationRepository) : IAddressVerificationService
    {
        private readonly IAddressVerificationRepository _addressVerificationRepository = addressVerificationRepository;

        /// <inheritdoc/>
        public async Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var response = await _addressVerificationRepository.GetVerifiedAddressesAsync(userId, cancellationToken);
            return response;
        }

        /// <inheritdoc/>
        public async Task<bool> SubmitVerificationCodeAsync(int userid, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken)
        {
            //var formattedAddress = VerificationCode.FormatAddress(address);

            bool VerifyFunc(string verificationCodeHash) => BCrypt.Net.BCrypt.Verify(verificationCode, verificationCodeHash);
            return _addressVerificationRepository.TryVerifyAddressAsync(userid, addressType, address, (Func<string, bool>)VerifyFunc, cancellationToken).Result;
        }
    }
}
