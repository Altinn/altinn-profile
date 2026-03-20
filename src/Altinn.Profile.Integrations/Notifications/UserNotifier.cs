using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User;

namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Sends user-facing notifications related to address changes and verification codes.
    /// Handles language resolution, message content building (via <see cref="UserMessageBuilder"/>),
    /// and delivery (via <see cref="INotificationsClient"/>).
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserNotifier"/> class.
    /// </remarks>
    /// <param name="notificationsClient">The notifications client for sending SMS and email.</param>
    /// <param name="userProfileService">The user profile service for resolving preferred language.</param>
    public class UserNotifier(INotificationsClient notificationsClient, IUserProfileService userProfileService) : IUserNotifier
    {
        private readonly INotificationsClient _notificationsClient = notificationsClient;
        private readonly IUserProfileService _userProfileService = userProfileService;

        /// <inheritdoc/>
        public async Task<bool> NotifyAddressChangeAsync(int userId, string address, AddressType addressType, Guid partyUuid, CancellationToken cancellationToken)
        {
            var language = await _userProfileService.GetPreferredLanguage(userId);
            var sendersReference = $"profile-{partyUuid}";

            if (addressType == AddressType.Sms)
            {
                var phoneNumberWithCountryCode = EnsureCountryCodeIfValidNumber(address);
                var body = UserMessageBuilder.GetSmsContent(language);
                return await _notificationsClient.OrderSmsAsync(phoneNumberWithCountryCode, body, sendersReference, cancellationToken);
            }
            else
            {
                var subject = UserMessageBuilder.GetEmailSubject(language);
                var body = UserMessageBuilder.GetEmailBody(language);
                return await _notificationsClient.OrderEmailAsync(address, subject, body, sendersReference, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SendVerificationCodeAsync(int userId, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken)
        {
            var language = await _userProfileService.GetPreferredLanguage(userId);
            var sendersReference = $"profile-{userId}-{addressType}-{DateTime.UtcNow.Ticks}";

            if (addressType == AddressType.Sms)
            {
                var phoneNumberWithCountryCode = EnsureCountryCodeIfValidNumber(address);
                var body = UserMessageBuilder.GetSmsContent(language, verificationCode);
                return await _notificationsClient.OrderSmsAsync(phoneNumberWithCountryCode, body, sendersReference, cancellationToken);
            }
            else
            {
                var subject = UserMessageBuilder.GetEmailSubject(language);
                var body = UserMessageBuilder.GetEmailBody(language, verificationCode);
                return await _notificationsClient.OrderEmailAsync(address, subject, body, sendersReference, cancellationToken);
            }
        }

        /// <summary>
        /// Checks if number contains country code, if not it adds the country code for Norway if number starts with 4 or 9
        /// </summary>
        /// <remarks>
        /// This method does not validate the number, only ensures that it has a country code.
        /// </remarks>
        public static string EnsureCountryCodeIfValidNumber(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber)) 
            {
                return mobileNumber;
            }
            else if (mobileNumber.StartsWith("00"))
            {
                mobileNumber = "+" + mobileNumber.Remove(0, 2);
            }
            else if (mobileNumber.Length == 8 && (mobileNumber[0] == '9' || mobileNumber[0] == '4'))
            {
                mobileNumber = "+47" + mobileNumber;
            }

            return mobileNumber;
        }
    }
}
