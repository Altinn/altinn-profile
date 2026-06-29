using Altinn.Profile.Core.Integrations;

using Microsoft.Extensions.Options;

namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// Service for managing self identified user contact information.
    /// </summary>
    /// <param name="userContactInfoRepository">Repository for user contact information data access.</param>
    /// <param name="registerClient">The register client for accessing user information.</param>
    public class UserContactInfoService(IUserContactInfoRepository userContactInfoRepository, IRegisterClient registerClient) : IUserContactInfoService
    {
        private readonly IUserContactInfoRepository _userContactInfoRepository = userContactInfoRepository;
        private readonly IRegisterClient _registerClient = registerClient;

        /// <inheritdoc/>
        public async Task<UserContactInfo?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken)
        {
            var userContactInfo = await _userContactInfoRepository.Get(userId, cancellationToken);
            if (userContactInfo == null)
            {
                return await CreateIfNotExists(userId, phoneNumber, cancellationToken);
            }

            return await _userContactInfoRepository.UpdatePhoneNumber(userId, phoneNumber, cancellationToken);
        }

        private async Task<UserContactInfo?> CreateIfNotExists(int userId, string? phoneNumber, CancellationToken cancellationToken)
        {
            var party = await _registerClient.GetUserParty(userId, cancellationToken);

            // Only email identified self identified users should have contact info created, if the user is not self identified we return null and do not create contact info for the user.
            var siuser = party as Register.Contracts.SelfIdentifiedUser;

            if (siuser == null || !siuser.User.HasValue)
            {
                return null;
            }

            var user = siuser.User.Value;

            var newUserContactInfo = new UserContactInfoCreateModel
            {
                UserId = userId,
                UserUuid = siuser.Uuid,
                Username = user.Username.Value ?? string.Empty,
                EmailAddress = siuser.Email.HasValue && siuser.Email.Value is not null
                    ? siuser.Email.Value : string.Empty,
                PhoneNumber = phoneNumber,
            };

            return await _userContactInfoRepository.CreateUserContactInfo(newUserContactInfo, cancellationToken);
        }     
    }
}
