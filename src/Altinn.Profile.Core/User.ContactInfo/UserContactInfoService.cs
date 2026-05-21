using System.Runtime;

using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

using Microsoft.Extensions.Options;

namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// Service for managing self identified user contact information.
    /// </summary>
    /// <param name="userContactInfoRepository">Repository for user contact information data access.</param>
    /// <param name="registerClient">The register client for accessing user information.</param>
    /// <param name="userProfileClient">The user profile client for accessing user profile information.</param>
    /// <param name="settings">Core settings for the application.</param>
    public class UserContactInfoService(IUserContactInfoRepository userContactInfoRepository, IRegisterClient registerClient, IUserProfileClient userProfileClient, IOptionsMonitor<CoreSettings> settings) : IUserContactInfoService
    {
        private readonly IUserContactInfoRepository _userContactInfoRepository = userContactInfoRepository;
        private readonly IRegisterClient _registerClient = registerClient;
        private readonly IUserProfileClient _userProfileClient = userProfileClient;
        private readonly CoreSettings _settings = settings.CurrentValue;

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
                if (_settings.SblBridgeFallbackEnabled)
                {
                    return await CreateFallbackUserContactInfo(userId, phoneNumber, cancellationToken);
                }

                return null;
            }

            var user = siuser.User.Value;

            var newUserContactInfo = new UserContactInfoCreateModel
            {
                UserId = userId,
                UserUuid = siuser.Uuid,
                Username = user?.Username.Value,
                EmailAddress = siuser.Email.HasValue && siuser.Email.Value is not null
       ? siuser.Email.Value : string.Empty,
                PhoneNumber = phoneNumber,
            };

            return await _userContactInfoRepository.CreateUserContactInfo(newUserContactInfo, cancellationToken);
        }

        private async Task<UserContactInfo?> CreateFallbackUserContactInfo(int userId, string? phoneNumber, CancellationToken cancellationToken)
        {
            var result = await _userProfileClient.GetUser(userId);
            var user = result.Match(
                user => user,
                _ => { return null; });

            var newUserContactInfo = new UserContactInfoCreateModel
            {
                UserId = userId,
                UserUuid = (Guid)user.UserUuid,
                Username = user.UserName,
                EmailAddress = user.Email ?? string.Empty,
                PhoneNumber = phoneNumber,
            };
            return await _userContactInfoRepository.CreateUserContactInfo(newUserContactInfo, cancellationToken);
        }
    }
}
