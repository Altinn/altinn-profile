using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ProfileSettings;

using Microsoft.Extensions.Options;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Service for handling professional notification addresses.
    /// </summary>
    public class ProfessionalNotificationsService(
        IProfessionalNotificationsRepository professionalNotificationsRepository,
        IUserProfileClient userProfileClient,
        INotificationsClient notificationsClient,
        IUserProfileService userProfileService,
        IOptions<AltinnConfiguration> altinnConfiguration) : IProfessionalNotificationsService
    {
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;
        private readonly IUserProfileClient _userProfileClient = userProfileClient;
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly INotificationsClient _notificationsClient = notificationsClient;
        private readonly AltinnConfiguration _altinnConfiguration = altinnConfiguration.Value;

        /// <inheritdoc/>
        public async Task<UserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            var notificationSettings = await _professionalNotificationsRepository.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);
            if (notificationSettings == null)
            {
                return null;
            }

            var profileSettings = await _userProfileService.GetProfileSettings(userId);
            if (NeedsConfirmation(notificationSettings, profileSettings))
            {
                notificationSettings.NeedsConfirmation = true;
            }

            return notificationSettings;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserPartyContactInfo>> GetAllNotificationAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var profileSettings = await _userProfileService.GetProfileSettings(userId);

            var notificationSettings = await _professionalNotificationsRepository.GetAllNotificationAddressesForUserAsync(userId, cancellationToken);
            foreach (var setting in notificationSettings)
            {
                if (NeedsConfirmation(setting, profileSettings))
                {
                    setting.NeedsConfirmation = true;
                }
            }

            return notificationSettings;
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
                await HandleNotificationAddressChangedAsync(contactInfo, mobileNumberChanged, emailChanged);
            }

            return isAdded;
        }

        /// <summary>
        /// Handles sending notifications when the mobile number or email address has changed.
        /// </summary>
        /// <param name="contactInfo">The updated contact info.</param>
        /// <param name="mobileNumberChanged">Indicates if the mobile number has changed.</param>
        /// <param name="emailChanged">Indicates if the email address has changed.</param>
        private async Task HandleNotificationAddressChangedAsync(UserPartyContactInfo contactInfo, bool mobileNumberChanged, bool emailChanged)
        {
            var userProfileResult = await _userProfileClient.GetUser(contactInfo.UserId);

            var language = userProfileResult.Match<string>(
                userProfile => userProfile.ProfileSettingPreference.Language,
                _ => "nb");

            if (mobileNumberChanged)
            {
                await _notificationsClient.OrderSms(contactInfo.PhoneNumber!, contactInfo.PartyUuid, language, CancellationToken.None);
            }

            if (emailChanged)
            {
                await _notificationsClient.OrderEmail(contactInfo.EmailAddress!, contactInfo.PartyUuid, language, CancellationToken.None);
            }
        }

        /// <inheritdoc/>
        public Task<UserPartyContactInfo?> DeleteNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.DeleteNotificationAddressAsync(userId, partyUuid, cancellationToken);
        }

        private bool NeedsConfirmation(UserPartyContactInfo notificationAddress, ProfileSettings? profileSettingPreference)
        {
            if (profileSettingPreference?.IgnoreUnitProfileDateTime.HasValue == true)
            {
                TimeSpan daysSinceIgnore = (TimeSpan)(DateTime.UtcNow - profileSettingPreference.IgnoreUnitProfileDateTime);
                if (daysSinceIgnore.TotalDays <= _altinnConfiguration.IgnoreUnitProfileConfirmationDays)
                {
                    return false;
                }
            }

            var lastModified = notificationAddress.LastChanged;

            var daysSinceLastUserUnitProfileUpdate = (DateTime.UtcNow - lastModified).TotalDays;
            if (daysSinceLastUserUnitProfileUpdate >= _altinnConfiguration.ValidationReminderDays)
            {
                // Altinn2 checks if the user is the "innehaver" of an "ENK" type org unit. We do not have that info here,
                return true;
            }

            return false;
        }
    }
}
