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
        public async Task NotifyAddressChangeAsync(int userId, string address, AddressType addressType, Guid partyUuid, CancellationToken cancellationToken)
        {
            var language = await _userProfileService.GetPreferredLanguage(userId);
            var sendersReference = partyUuid.ToString();

            if (addressType == AddressType.Sms)
            {
                var body = UserMessageBuilder.GetSmsContent(language);
                await _notificationsClient.OrderSmsAsync(address, body, sendersReference, cancellationToken);
            }
            else
            {
                var subject = UserMessageBuilder.GetEmailSubject(language);
                var body = UserMessageBuilder.GetEmailBody(language);
                await _notificationsClient.OrderEmailAsync(address, subject, body, sendersReference, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task SendVerificationCodeAsync(int userId, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken)
        {
            var language = await _userProfileService.GetPreferredLanguage(userId);
            var sendersReference = $"{userId}-{addressType}-{DateTime.UtcNow.Ticks}";

            if (addressType == AddressType.Sms)
            {
                var body = UserMessageBuilder.GetSmsContent(language, verificationCode);
                await _notificationsClient.OrderSmsAsync(address, body, sendersReference, cancellationToken);
            }
            else
            {
                var subject = UserMessageBuilder.GetEmailSubject(language);
                var body = UserMessageBuilder.GetEmailBody(language, verificationCode);
                await _notificationsClient.OrderEmailAsync(address, subject, body, sendersReference, cancellationToken);
            }
        }
    }
}
