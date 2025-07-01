using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Service for handling professional notification addresses.
    /// </summary>
    public class ProfessionalNotificationsService(IProfessionalNotificationsRepository professionalNotificationsRepository, IUserProfileClient userProfileClient, INotificationsClient notificationsClient) : IProfessionalNotificationsService
    {
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;
        private readonly IUserProfileClient _userProfileClient = userProfileClient;
        private readonly INotificationsClient _notificationsClient = notificationsClient;

        /// <inheritdoc/>
        public Task<UserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> AddOrUpdateNotificationAddressAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken)
        {
            var existingContactInfo = await _professionalNotificationsRepository.GetNotificationAddressAsync(contactInfo.UserId, contactInfo.PartyUuid, cancellationToken);

            var mobileNumberChanged = !string.IsNullOrWhiteSpace(contactInfo.PhoneNumber) && existingContactInfo?.PhoneNumber != contactInfo.PhoneNumber;
            var emailChanged = !string.IsNullOrWhiteSpace(contactInfo.EmailAddress) && existingContactInfo?.EmailAddress != contactInfo.EmailAddress;

            var isAdded = await _professionalNotificationsRepository.AddOrUpdateNotificationAddressAsync(contactInfo, cancellationToken);

            if (mobileNumberChanged || emailChanged)
            {
                var userProfileResult = await _userProfileClient.GetUser(contactInfo.UserId);

                var language = userProfileResult.Match<string>(
                    userProfile => userProfile.ProfileSettingPreference.Language,
                    _ => "nb");

                if (mobileNumberChanged)
                {
                    await _notificationsClient.SendSmsOrder(contactInfo.PhoneNumber!, language, CancellationToken.None);
                }

                if (emailChanged)
                {
                    await _notificationsClient.SendEmailOrder(contactInfo.EmailAddress!, language, CancellationToken.None);
                }
            }

            return isAdded;
        }

        /// <inheritdoc/>
        public Task<UserPartyContactInfo?> DeleteNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.DeleteNotificationAddressAsync(userId, partyUuid, cancellationToken);
        }
    }
}
