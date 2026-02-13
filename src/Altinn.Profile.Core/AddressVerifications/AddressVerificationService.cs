using System.Security.Cryptography;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// A service for handling address verification processes, including generating and sending verification codes via email or SMS.
    /// </summary>
    public class AddressVerificationService(INotificationsClient notificationsClient, IAddressVerificationRepository addressVerificationRepository) : IAddressVerificationService
    {
        private readonly INotificationsClient _notificationsClient = notificationsClient;
        private readonly IAddressVerificationRepository _addressVerificationRepository = addressVerificationRepository;
        private readonly int _expiryTimeInMinutes = 15;

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
            var formattedAddress = VerificationCode.FormatAddress(address);
            var verificationCode = GenerateVerificationCode();

            var verificationCodeHash = BCrypt.Net.BCrypt.HashPassword(verificationCode);
            var verificationCodeModel = new VerificationCode
            {
                UserId = userid,
                Address = formattedAddress,
                AddressType = addressType,
                VerificationCodeHash = verificationCodeHash,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_expiryTimeInMinutes),
            };

            await _addressVerificationRepository.AddNewVerificationCodeAsync(verificationCodeModel);
            if (addressType == AddressType.Email)
            {
                await _notificationsClient.OrderEmailWithCode(formattedAddress, partyUuid, languageCode, verificationCode, cancellationToken);
            }
            else if (addressType == AddressType.Sms)
            {
                await _notificationsClient.OrderSmsWithCode(formattedAddress, partyUuid, languageCode, verificationCode, cancellationToken);
            }
        }

        private static string GenerateVerificationCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        /// <inheritdoc/>
        public async Task NotifySmsAddressChangeAsync(string phoneNumber, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken)
        {
            await _notificationsClient.OrderSms(phoneNumber, partyUuid, languageCode, cancellationToken);
            await _addressVerificationRepository.AddLegacyAddressAsync(AddressType.Sms, phoneNumber, userid, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task NotifyEmailAddressChangeAsync(string emailAddress, Guid partyUuid, string languageCode, int userid, CancellationToken cancellationToken)
        {
            await _notificationsClient.OrderEmail(emailAddress, partyUuid, languageCode, cancellationToken);
            await _addressVerificationRepository.AddLegacyAddressAsync(AddressType.Email, emailAddress, userid, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var response = await _addressVerificationRepository.GetVerifiedAddressesAsync(userId, cancellationToken);
            return response;
        }
    }
}
