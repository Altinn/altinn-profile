using System.Linq;

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
        IUserProfileClient userProfileClient,
        INotificationsClient notificationsClient,
        IUserProfileService userProfileService,
        IRegisterClient registerClient,
        IOptions<AddressMaintenanceSettings> addressMaintenanceSettings,
        IAddressVerificationRepository addressVerificationRepository) : IProfessionalNotificationsService
    {
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;
        private readonly IUserProfileClient _userProfileClient = userProfileClient;
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly INotificationsClient _notificationsClient = notificationsClient;
        private readonly IRegisterClient _registerClient = registerClient;
        private readonly AddressMaintenanceSettings _addressMaintenanceSettings = addressMaintenanceSettings.Value;
        private readonly IAddressVerificationRepository _addressVerificationRepository = addressVerificationRepository;

        /// <inheritdoc/>
        public async Task<ExtendedUserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            var notificationSettings = await _professionalNotificationsRepository.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);
            if (notificationSettings == null)
            {
                return null;
            }

            var extendedInfo = new ExtendedUserPartyContactInfo(notificationSettings);

            await EnrichWithVerificationStatus(extendedInfo, cancellationToken);

            var profileSettings = await _userProfileService.GetProfileSettings(userId);
            if (NeedsConfirmation(extendedInfo, profileSettings))
            {
                extendedInfo.NeedsConfirmation = true;
            }

            return extendedInfo;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ExtendedUserPartyContactInfo>> GetAllNotificationAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var profileSettings = await _userProfileService.GetProfileSettings(userId);

            var notificationSettings = await _professionalNotificationsRepository.GetAllNotificationAddressesForUserAsync(userId, cancellationToken);

            var extendedInfoList = notificationSettings.Select(ns => new ExtendedUserPartyContactInfo(ns)).ToList();

            foreach (var extendedInfo in extendedInfoList)
            {
                await EnrichWithVerificationStatus(extendedInfo, cancellationToken);
                if (NeedsConfirmation(extendedInfo, profileSettings))
                {
                    extendedInfo.NeedsConfirmation = true;
                }
            }

            return extendedInfoList;
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

        private bool NeedsConfirmation(UserPartyContactInfo notificationAddress, ProfileSettings? profileSettingPreference)
        {
            if (profileSettingPreference?.IgnoreUnitProfileDateTime.HasValue == true)
            {
                TimeSpan daysSinceIgnore = (TimeSpan)(DateTime.UtcNow - profileSettingPreference.IgnoreUnitProfileDateTime);
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

        private async Task EnrichWithVerificationStatus(ExtendedUserPartyContactInfo notificationAddress, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(notificationAddress.EmailAddress))
            {
                var emailResult = await _addressVerificationRepository.GetVerificationStatus(notificationAddress.UserId, AddressType.Email, notificationAddress.EmailAddress, cancellationToken);
                notificationAddress.EmailVerificationStatus = emailResult ?? VerificationType.Unverified;
            }

            if (!string.IsNullOrWhiteSpace(notificationAddress.PhoneNumber))
            {
                var smsResult = await _addressVerificationRepository.GetVerificationStatus(notificationAddress.UserId, AddressType.Sms, notificationAddress.PhoneNumber, cancellationToken);
                notificationAddress.SmsVerificationStatus = smsResult ?? VerificationType.Unverified;
            }
        }
    }
}
