using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// Service for managing self identified user contact information.
    /// </summary>
    /// <param name="userContactInfoRepository">Repository for user contact information data access.</param>
    public class UserContactInfoService(IUserContactInfoRepository userContactInfoRepository) : IUserContactInfoService
    {
        private readonly IUserContactInfoRepository _userContactInfoRepository = userContactInfoRepository;

        /// <inheritdoc/>
        public async Task<UserContactInfo?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken)
        {
            return await _userContactInfoRepository.UpdatePhoneNumber(userId, phoneNumber, cancellationToken);
        }
    }
}
