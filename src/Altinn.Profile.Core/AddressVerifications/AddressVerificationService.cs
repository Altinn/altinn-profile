using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

using Microsoft.Extensions.Options;

namespace Altinn.Profile.Core.AddressVerifications
{
    /// <summary>
    /// A service for handling address verification processes, including generating verification codes,
    /// persisting them, and delegating notification delivery to <see cref="IUserNotifier"/>.
    /// </summary>
    public class AddressVerificationService(
        IUserNotifier userNotifier,
        IAddressVerificationRepository addressVerificationRepository,
        IVerificationCodeService verificationCodeService,
        IOptions<AddressMaintenanceSettings> addressMaintenanceSettings,
        Telemetry.Telemetry telemetry) : IAddressVerificationService
    {
        private readonly IUserNotifier _userNotifier = userNotifier;
        private readonly IAddressVerificationRepository _addressVerificationRepository = addressVerificationRepository;
        private readonly IVerificationCodeService _verificationCodeService = verificationCodeService;
        private readonly IOptions<AddressMaintenanceSettings> _addressMaintenanceSettings = addressMaintenanceSettings;
        private readonly Telemetry.Telemetry _telemetry = telemetry;

        /// <inheritdoc/>
        public async Task<(VerificationType? EmailVerificationStatus, VerificationType? SmsVerificationStatus)> GetVerificationStatusAsync(int userId, string? emailAddress, string? phoneNumber, CancellationToken cancellationToken)
        {
            VerificationType? emailVerificationStatus = null;
            VerificationType? smsVerificationStatus = null;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailVerificationStatus = await _addressVerificationRepository.GetVerificationStatusAsync(userId, AddressType.Email, emailAddress, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                smsVerificationStatus = await _addressVerificationRepository.GetVerificationStatusAsync(userId, AddressType.Sms, phoneNumber, cancellationToken);
            }

            return (emailVerificationStatus, smsVerificationStatus);
        }

        /// <inheritdoc/>
        public async Task GenerateAndSendVerificationCodeAsync(int userid, string address, AddressType addressType, CancellationToken cancellationToken)
        {
            var existingVerification = await _addressVerificationRepository.GetVerificationStatusAsync(userid, addressType, address, cancellationToken);
            if (existingVerification == VerificationType.Verified)
            {
                // If the address is already verified, we don't need to generate a new code or send a notification.
                return;
            }

            var code = _verificationCodeService.GenerateRawCode();
            var verificationCodeModel = _verificationCodeService.CreateVerificationCode(userid, address, addressType, code);

            bool added = await _addressVerificationRepository.AddNewVerificationCodeAsync(verificationCodeModel);
            if (!added)
            {
                // A concurrent request already inserted a verification code for this user/address/type.
                // Discard this code and skip sending the notification.
                return;
            }

            await _userNotifier.SendVerificationCodeAsync(userid, verificationCodeModel.Address, addressType, code, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken)
        {
            var response = await _addressVerificationRepository.GetVerifiedAddressesAsync(userId, cancellationToken);
            return response;
        }

        /// <inheritdoc/>
        public async Task<bool> SubmitVerificationCodeAsync(int userid, string address, AddressType addressType, string submittedCode, CancellationToken cancellationToken)
        {
            var formattedAddress = VerificationCode.FormatAddress(address);

            var storedCode = await _addressVerificationRepository.GetVerificationCodeAsync(userid, addressType, formattedAddress, cancellationToken);

            if (storedCode is null)
            {
                return false;
            }

            if (_verificationCodeService.VerifyCode(submittedCode, storedCode))
            {
                await _addressVerificationRepository.CompleteAddressVerificationAsync(storedCode.VerificationCodeId, addressType, formattedAddress, userid);
                return true;
            }
            else
            {
                await _addressVerificationRepository.IncrementFailedAttemptsAsync(storedCode.VerificationCodeId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<ResendVerificationResult> ResendVerificationCodeAsync(int userId, string address, AddressType addressType, CancellationToken cancellationToken)
        {
            var formattedAddress = VerificationCode.FormatAddress(address);

            var existingCode = await _addressVerificationRepository.GetVerificationCodeAsync(userId, addressType, formattedAddress, cancellationToken);
            if (existingCode is null)
            {
                _telemetry.RecordVerificationResendCodeNotFound(addressType);
                return ResendVerificationResult.CodeNotFound;
            }

            RecordResendPatienceTelemetry(addressType, existingCode.Created);

            var isExistingCodeInCooldown = existingCode.Created + TimeSpan.FromSeconds(_addressMaintenanceSettings.Value.VerificationCodeResendCooldownSeconds) > DateTime.UtcNow;

            if (isExistingCodeInCooldown)
            {
                _telemetry.RecordVerificationResendCooldownRejected(addressType);
                return ResendVerificationResult.CodeCooldown; // Don't generate a new code or send a notification if there's an existing code in the cooldown state
            }

            var code = _verificationCodeService.GenerateRawCode();
            var verificationCodeModel = _verificationCodeService.CreateVerificationCode(userId, formattedAddress, addressType, code);

            bool added = await _addressVerificationRepository.AddNewVerificationCodeAsync(verificationCodeModel);
            if (!added)
            {
                // A concurrent request already inserted a verification code for this user/address/type.
                return ResendVerificationResult.CodeCooldown;
            }

            await _userNotifier.SendVerificationCodeAsync(userId, verificationCodeModel.Address, addressType, code, cancellationToken);

            return ResendVerificationResult.Success;
        }

        private void RecordResendPatienceTelemetry(AddressType addressType, DateTime codeCreated)
        {
            double secondsWaited = (DateTime.UtcNow - codeCreated).TotalSeconds;
            _telemetry.RecordResendPatience(secondsWaited, addressType);
        }
    }
}
