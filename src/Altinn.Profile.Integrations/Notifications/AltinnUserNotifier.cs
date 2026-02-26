using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User;

namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Sends user-facing notifications related to address changes and verification codes.
    /// Handles language resolution, message content building (via <see cref="AltinnUserMessageBuilder"/>),
    /// and delivery (via <see cref="INotificationsClient"/>).
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AltinnUserNotifier"/> class.
    /// </remarks>
    /// <param name="notificationsClient">The notifications client for sending SMS and email.</param>
    /// <param name="userProfileService">The user profile service for resolving preferred language.</param>
    public class AltinnUserNotifier(INotificationsClient notificationsClient, IUserProfileService userProfileService) : IAltinnUserNotifier
    {
        private readonly INotificationsClient _notificationsClient = notificationsClient;
        private readonly IUserProfileService _userProfileService = userProfileService;

        /// <inheritdoc/>
        public async Task NotifyAddressChangeAsync(int userId, string address, AddressType addressType, Guid partyUuid, CancellationToken cancellationToken)
        {
            var language = await _userProfileService.GetPreferredLanguage(userId);
            var sendersReference = partyUuid.ToString();

            if (addressType == AddressType.Sms)
            {
                var body = AltinnUserMessageBuilder.GetSmsContent(language);
                await _notificationsClient.OrderSms(address, body, sendersReference, cancellationToken);
            }
            else
            {
                var subject = AltinnUserMessageBuilder.GetEmailSubject(language);
                var body = AltinnUserMessageBuilder.GetEmailBody(language);
                await _notificationsClient.OrderEmail(address, subject, body, sendersReference, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task SendVerificationCodeAsync(int userId, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken)
        {
            var language = await _userProfileService.GetPreferredLanguage(userId);
            var sendersReference = $"{userId}:{address}";

            if (addressType == AddressType.Sms)
            {
                var body = AltinnUserMessageBuilder.GetSmsContent(language, verificationCode);
                await _notificationsClient.OrderSms(address, body, sendersReference, cancellationToken);
            }
            else
            {
                var subject = AltinnUserMessageBuilder.GetEmailSubject(language);
                var body = AltinnUserMessageBuilder.GetEmailBody(language, verificationCode);
                await _notificationsClient.OrderEmail(address, subject, body, sendersReference, cancellationToken);
            }
        }
    }
}
