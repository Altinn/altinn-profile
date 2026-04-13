using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// Service for managing self identified user contact information.
    /// </summary>
    /// <param name="userContactInfoRepository">Repository for user contact information data access.</param>
    /// <param name="addressVerificationService">Service for address verification operations.</param>
    public class UserContactInfoService(IUserContactInfoRepository userContactInfoRepository, IAddressVerificationService addressVerificationService) : IUserContactInfoService
    {
        private readonly IUserContactInfoRepository _userContactInfoRepository = userContactInfoRepository;
        private readonly IAddressVerificationService _addressVerificationService = addressVerificationService;

        /// <summary>
        /// Checks if the phone number has been verified, or if the phone number is null. If the phone number is null, this method returns true, as there is no phone number to verify.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="phoneNumber">The phone number to check.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the phone number is verified or null, otherwise false.</returns>
        public async Task<bool> IsAddressVerifiedOrNull(int userId, string phoneNumber, CancellationToken cancellationToken)
        {
            if (phoneNumber != null)
            {
                var smsVerificationStatus = await _addressVerificationService.GetVerificationStatusAsync(userId, AddressType.Sms, phoneNumber, cancellationToken);
                if (smsVerificationStatus.HasValue && smsVerificationStatus != VerificationType.Verified)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<string?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken)
        {
            var updatedContactInfo = await _userContactInfoRepository.UpdatePhoneNumber(userId, phoneNumber, cancellationToken);
            return updatedContactInfo?.PhoneNumber;
        }
    }
}
