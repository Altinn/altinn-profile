using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// A service for handling address verification processes, including generating and sending verification codes via email or SMS.
    /// </summary>
    public class AddressVerificationService(INotificationsClient notificationsClient, IAddressVerificationRepository addressVerificationRepository, IVerificationCodeService verificationCodeService) : IAddressVerificationService
    {
        private readonly INotificationsClient _notificationsClient = notificationsClient;
        private readonly IAddressVerificationRepository _addressVerificationRepository = addressVerificationRepository;
        private readonly IVerificationCodeService _verificationCodeService = verificationCodeService;

        /// <inheritdoc/>
        public async Task<(VerificationType? EmailVerificationStatus, VerificationType? SmsVerificationStatus)> GetVerificationStatusAsync(int userId, string? emailAddress, string? phoneNumber, CancellationToken cancellationToken)
        {
            VerificationType? emailVerificationStatus = null;
            VerificationType? smsVerificationStatus = null;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                var emailResult = await _addressVerificationRepository.GetVerificationStatusAsync(userId, AddressType.Email, emailAddress, cancellationToken);
                emailVerificationStatus = emailResult ?? VerificationType.Unverified;
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var smsResult = await _addressVerificationRepository.GetVerificationStatusAsync(userId, AddressType.Sms, phoneNumber, cancellationToken);
                smsVerificationStatus = smsResult ?? VerificationType.Unverified;
            }

            return (emailVerificationStatus, smsVerificationStatus);
        }

        /// <inheritdoc/>
        public async Task GenerateAndSendVerificationCodeAsync(int userid, string address, AddressType addressType, string languageCode, Guid partyUuid, CancellationToken cancellationToken)
        {
            var existingVerification = await _addressVerificationRepository.GetVerificationStatusAsync(userid, addressType, address, cancellationToken);
            if (existingVerification == VerificationType.Verified)
            {
                // If the address is already verified, we don't need to generate a new code or send a notification.
                return;
            }

            var code = _verificationCodeService.GenerateRawCode();
            var verificationCodeModel = _verificationCodeService.CreateVerificationCode(userid, address, addressType, code);

            await _addressVerificationRepository.AddNewVerificationCodeAsync(verificationCodeModel);
            if (addressType == AddressType.Email)
            {
                await _notificationsClient.OrderEmailWithCode(verificationCodeModel.Address, partyUuid, languageCode, code, cancellationToken);
            }
            else if (addressType == AddressType.Sms)
            {
                await _notificationsClient.OrderSmsWithCode(verificationCodeModel.Address, partyUuid, languageCode, code, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task NotifySmsAddressChangeAsync(string phoneNumber, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken)
        {
            // For legacy addresses, we want to keep track of them in the database for a certain period of time, but we don't want to generate a verification code or send a notification for them.
            // This can be deleted when verification is required for all new addresses
            await _addressVerificationRepository.AddLegacyAddressAsync(AddressType.Sms, phoneNumber, userid, cancellationToken);
            await _notificationsClient.OrderSms(phoneNumber, partyUuid, languageCode, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task NotifyEmailAddressChangeAsync(string emailAddress, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken)
        {
            // For legacy addresses, we want to keep track of them in the database for a certain period of time, but we don't want to generate a verification code or send a notification for them.
            // This can be deleted when verification is required for all new addresses
            await _addressVerificationRepository.AddLegacyAddressAsync(AddressType.Email, emailAddress, userid, cancellationToken);
            await _notificationsClient.OrderEmail(emailAddress, partyUuid, languageCode, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var response = await _addressVerificationRepository.GetVerifiedAddressesAsync(userId, cancellationToken);
            return response;
        }
    }
}
