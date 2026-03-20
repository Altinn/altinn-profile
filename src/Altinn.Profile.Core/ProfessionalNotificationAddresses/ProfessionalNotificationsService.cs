using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
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
        IUserProfileService userProfileService,
        IRegisterClient registerClient,
        IOptions<AddressMaintenanceSettings> addressMaintenanceSettings,
        IAddressVerificationService addressVerificationService,
        IUserNotifier userNotifier) : IProfessionalNotificationsService
    {
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly IRegisterClient _registerClient = registerClient;
        private readonly AddressMaintenanceSettings _addressMaintenanceSettings = addressMaintenanceSettings.Value;
        private readonly IAddressVerificationService _addressVerificationService = addressVerificationService;
        private readonly IUserNotifier _userNotifier = userNotifier;

        /// <inheritdoc/>
        public async Task<ExtendedUserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            var notificationSettings = await _professionalNotificationsRepository.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);
            if (notificationSettings == null)
            {
                return null;
            }

            var (emailVerificationStatus, smsVerificationStatus) = await _addressVerificationService.GetVerificationStatusAsync(notificationSettings.UserId, notificationSettings.EmailAddress, notificationSettings.PhoneNumber, cancellationToken);
            var ignoreUnitProfileDateTime = await _userProfileService.GetIgnoreUnitProfileDateTime(userId);

            var needsConfirmation = NeedsConfirmation(notificationSettings, ignoreUnitProfileDateTime);
            var extendedInfo = new ExtendedUserPartyContactInfo(
                    notificationSettings,
                    needsConfirmation,
                    emailVerificationStatus,
                    smsVerificationStatus);

            return extendedInfo;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ExtendedUserPartyContactInfo>> GetAllNotificationAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var ignoreUnitProfileDateTime = await _userProfileService.GetIgnoreUnitProfileDateTime(userId);

            var notificationSettings = await _professionalNotificationsRepository.GetAllNotificationAddressesForUserAsync(userId, cancellationToken);

            List<ExtendedUserPartyContactInfo> enrichedNotificationSettings = [];

            foreach (var notificationSetting in notificationSettings)
            {
                var needsConfirmation = NeedsConfirmation(notificationSetting, ignoreUnitProfileDateTime);
                var (emailVerificationStatus, smsVerificationStatus) = await _addressVerificationService.GetVerificationStatusAsync(notificationSetting.UserId, notificationSetting.EmailAddress, notificationSetting.PhoneNumber, cancellationToken);
                enrichedNotificationSettings.Add(new ExtendedUserPartyContactInfo(
                    notificationSetting,
                    needsConfirmation,
                    emailVerificationStatus,
                    smsVerificationStatus));
            }

            return enrichedNotificationSettings;
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

        /// <inheritdoc/>
        public async Task<bool> AddOrUpdateNotificationAddressAsync(PatchUserPartyContactInfo contactInfo, CancellationToken cancellationToken)
        {
            var existingContactInfo = await _professionalNotificationsRepository.GetNotificationAddressAsync(contactInfo.UserId, contactInfo.PartyUuid, cancellationToken);

            var updatedContactInfo = new UserPartyContactInfo
            {
                UserId = contactInfo.UserId,
                PartyUuid = contactInfo.PartyUuid,
                EmailAddress = contactInfo.EmailAddress.HasValue ? contactInfo.EmailAddress.Value : existingContactInfo?.EmailAddress,
                PhoneNumber = contactInfo.PhoneNumber.HasValue ? contactInfo.PhoneNumber.Value : existingContactInfo?.PhoneNumber,
                UserPartyContactInfoResources = contactInfo.UserPartyContactInfoResources.HasValue ? contactInfo.UserPartyContactInfoResources.Value : existingContactInfo?.UserPartyContactInfoResources
            };

            var mobileNumberChanged = !string.IsNullOrWhiteSpace(updatedContactInfo.PhoneNumber) && existingContactInfo?.PhoneNumber != updatedContactInfo.PhoneNumber;
            var emailChanged = !string.IsNullOrWhiteSpace(updatedContactInfo.EmailAddress) && existingContactInfo?.EmailAddress != updatedContactInfo.EmailAddress;

            var isAdded = await _professionalNotificationsRepository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, cancellationToken);

            if (mobileNumberChanged || emailChanged)
            {
                // Verification flow is allways activated now, but we don't send a notification if the address is already verified
                await HandleNotificationAddressChangedAsync(updatedContactInfo, mobileNumberChanged, emailChanged);
            }

            return isAdded;
        }

        /// <summary>
        /// Handles sending notifications when the mobile number or email address has changed.
        /// Verification code generation is delegated to <see cref="IAddressVerificationService"/>;
        /// address-change notifications are sent directly via <see cref="IUserNotifier"/>.
        /// </summary>
        /// <param name="contactInfo">The updated contact info.</param>
        /// <param name="mobileNumberChanged">Indicates if the mobile number has changed.</param>
        /// <param name="emailChanged">Indicates if the email address has changed.</param>
        private async Task HandleNotificationAddressChangedAsync(UserPartyContactInfo contactInfo, bool mobileNumberChanged, bool emailChanged)
        {
            // The request processing will reach this point in the flow only if cancellation has not yet occurred.
            // Should the client cancel after this point, we still want the remaining operations proceed
            var emptyCancellationToken = CancellationToken.None;

            if (mobileNumberChanged)
            {
                await _addressVerificationService.GenerateAndSendVerificationCodeAsync(contactInfo.UserId, contactInfo.PhoneNumber!, AddressType.Sms, emptyCancellationToken);
            }

            if (emailChanged)
            {
                await _addressVerificationService.GenerateAndSendVerificationCodeAsync(contactInfo.UserId, contactInfo.EmailAddress!, AddressType.Email, emptyCancellationToken);
            }
        }

        /// <inheritdoc/>
        public Task<UserPartyContactInfo?> DeleteNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.DeleteNotificationAddressAsync(userId, partyUuid, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserPartyContactInfoWithIdentity>?> GetContactInformationByOrganizationNumberAsync(string organizationNumber, CancellationToken cancellationToken)
        {
            // Step 1: Translate orgNumber to partyUuid
            var parties = await _registerClient.GetPartyUuids([organizationNumber], cancellationToken);

            if (parties == null || parties.Count == 0)
            {
                return [];
            }

            if (parties.Count > 1)
            {
                throw new InvalidOperationException("Indecisive organization result");
            }

            var partyUuid = parties[0].PartyUuid;

            // Step 2: Get all user contact info for this party
            var contactInfos = await _professionalNotificationsRepository
                .GetAllNotificationAddressesForPartyAsync(partyUuid, cancellationToken) ?? [];

            var results = new List<UserPartyContactInfoWithIdentity>();

            // Sequential execution is acceptable here because:
            // 1. This is a Support Dashboard endpoint (low traffic, not high-throughput)
            // 2. Expected cardinality is small (typically few users per organization)
            // 3. IUserProfileService.GetUser only supports individual lookups (no batch API for userId)
            foreach (var contactInfo in contactInfos)
            {
                // Note: IUserProfileService.GetUser does not support cancellation token at this time
                var userProfileResult = await _userProfileService.GetUser(contactInfo.UserId);

                userProfileResult.Match(
                    profile =>
                    {
                        // Skip if Party data is missing or incomplete (consistent with FilterAndMapAddresses pattern)
                        if (profile.Party == null || string.IsNullOrEmpty(profile.Party.Name))
                        {
                            return;
                        }

                        results.Add(new UserPartyContactInfoWithIdentity
                        {
                            NationalIdentityNumber = profile.Party.SSN,
                            Name = profile.Party.Name,
                            EmailAddress = contactInfo.EmailAddress,
                            PhoneNumber = contactInfo.PhoneNumber,
                            OrganizationNumber = organizationNumber,
                            LastChanged = contactInfo.LastChanged
                        });
                    },
                    _ =>
                    {
                        // Failed to retrieve user profile - skip this user
                    });
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserPartyContactInfoWithIdentity>> GetContactInformationByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken)
        {
            var listOfContactInfosForEmailAddress = await _professionalNotificationsRepository.GetAllContactInfoByEmailAddressAsync(emailAddress, cancellationToken) ?? new List<UserPartyContactInfo>();

            if (listOfContactInfosForEmailAddress.Count == 0)
            {
                return [];
            }

            return await BuildContactInfosWithIdentityAsync(listOfContactInfosForEmailAddress, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserPartyContactInfoWithIdentity>> GetContactInformationByPhoneNumberAsync(string phoneNumber, string countryCode, CancellationToken cancellationToken)
        {
            var fullPhoneNumber = countryCode + phoneNumber;

            var listOfContactInfosForPhoneNumber = await _professionalNotificationsRepository.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, cancellationToken) ?? new List<UserPartyContactInfo>();

            if (listOfContactInfosForPhoneNumber.Count == 0)
            {
                return [];
            }

            return await BuildContactInfosWithIdentityAsync(listOfContactInfosForPhoneNumber, cancellationToken);
        }

        /// <summary>
        /// Build enriched contact info list by mapping repository contact infos to identity enriched objects.
        /// </summary>
        private async Task<List<UserPartyContactInfoWithIdentity>> BuildContactInfosWithIdentityAsync(IEnumerable<UserPartyContactInfo> contactInfos, CancellationToken cancellationToken)
        {
            var results = new List<UserPartyContactInfoWithIdentity>();

            foreach (var contactInfo in contactInfos)
            {
                var orgNumber = await _registerClient.GetOrganizationNumberByPartyUuid(contactInfo.PartyUuid, cancellationToken);

                // Note: IUserProfileService.GetUser does not support cancellation token at this time
                var userProfileResult = await _userProfileService.GetUser(contactInfo.UserId);

                userProfileResult.Match(
                    profile =>
                    {
                        // Skip if Party data is missing or incomplete 
                        if (profile.Party == null || string.IsNullOrEmpty(profile.Party.Name))
                        {
                            return;
                        }

                        results.Add(new UserPartyContactInfoWithIdentity
                        {
                            NationalIdentityNumber = profile.Party.SSN,
                            Name = profile.Party.Name,
                            EmailAddress = contactInfo.EmailAddress,
                            PhoneNumber = contactInfo.PhoneNumber,
                            OrganizationNumber = orgNumber,
                            LastChanged = contactInfo.LastChanged
                        });
                    },
                    _ =>
                    {
                        // Failed to retrieve user profile - skip this user
                    });
            }

            return results;
        }

        private bool NeedsConfirmation(UserPartyContactInfo notificationAddress, DateTime? ignoreUnitProfileDateTime)
        {
            if (ignoreUnitProfileDateTime.HasValue)
            {
                TimeSpan daysSinceIgnore = (TimeSpan)(DateTime.UtcNow - ignoreUnitProfileDateTime);
                if (daysSinceIgnore.TotalDays <= _addressMaintenanceSettings.IgnoreUnitProfileConfirmationDays)
                {
                    return false;
                }
            }

            var lastModified = notificationAddress.LastChanged;

            var daysSinceLastUserUnitProfileUpdate = (DateTime.UtcNow - lastModified).TotalDays;
            if (daysSinceLastUserUnitProfileUpdate >= _addressMaintenanceSettings.ValidationReminderDays)
            {
                // Altinn2 checks if the user is the "innehaver" of an "ENK" type org unit. We do not have that info here,
                return true;
            }

            return false;
        }
    }
}
