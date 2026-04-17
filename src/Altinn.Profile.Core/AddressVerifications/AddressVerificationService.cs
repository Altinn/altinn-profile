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
        private readonly int _resendCoolDownSeconds = addressMaintenanceSettings.Value.VerificationCodeResendCooldownSeconds;
        private readonly Telemetry.Telemetry _telemetry = telemetry;

        /// <inheritdoc/>
        public async Task<(VerificationType? EmailVerificationStatus, VerificationType? SmsVerificationStatus)> GetVerificationStatusAsync(int userId, string? emailAddress, string? phoneNumber, CancellationToken cancellationToken)
        {
            VerificationType? emailVerificationStatus = await GetVerificationStatusAsync(userId, AddressType.Email, emailAddress, cancellationToken);
            VerificationType? smsVerificationStatus = await GetVerificationStatusAsync(userId, AddressType.Sms, phoneNumber, cancellationToken);

            return (emailVerificationStatus, smsVerificationStatus);
        }

        /// <inheritdoc/>
        public async Task<VerificationType?> GetVerificationStatusAsync(int userId, AddressType addressType, string? address, CancellationToken cancellationToken)
        {
            VerificationType? verificationStatus = null;
            if (!string.IsNullOrWhiteSpace(address))
            {
                verificationStatus = await _addressVerificationRepository.GetVerificationStatusAsync(userId, addressType, address, cancellationToken);
            }

            return verificationStatus;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAddressVerifiedOrNull(int userId, AddressType addressType, string? address, CancellationToken cancellationToken)
        {
            var verificationStatus = await GetVerificationStatusAsync(userId, addressType, address, cancellationToken);
            return verificationStatus == null || verificationStatus == VerificationType.Verified;
        }

        /// <inheritdoc/>
        /// This method might be deleted at a later time when all callers have migrated to using SendVerificationCodeAsync, which includes cooldown logic and resend functionality.
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
        public async Task<SendVerificationCodeResult> SendVerificationCodeAsync(int userId, string address, AddressType addressType, CancellationToken cancellationToken)
        {
            var formattedAddress = VerificationCode.FormatAddress(address);

            var existingVerification = await _addressVerificationRepository.GetVerificationStatusAsync(userId, addressType, formattedAddress, cancellationToken);
            if (existingVerification == VerificationType.Verified)
            {
                return SendVerificationCodeResult.AlreadyVerified();
            }

            var existingCode = await _addressVerificationRepository.GetVerificationCodeAsync(userId, addressType, formattedAddress, cancellationToken);
            if (existingCode is not null && IsInCooldown(existingCode, out double secondsWaited))
            {
                var remainingCoolDownTime = _resendCoolDownSeconds - (int)secondsWaited;
                _telemetry.RecordVerificationResendCooldownRejected(addressType);
                return SendVerificationCodeResult.CoolDown(remainingCoolDownTime);
            }

            var code = _verificationCodeService.GenerateRawCode();
            var verificationCodeModel = _verificationCodeService.CreateVerificationCode(userId, formattedAddress, addressType, code);

            bool added = await _addressVerificationRepository.AddNewVerificationCodeAsync(verificationCodeModel);
            if (!added)
            {
                // A concurrent request already inserted a verification code for this user/address/type.
                return SendVerificationCodeResult.CoolDown(_resendCoolDownSeconds);
            }

            var notificationSent = await _userNotifier.SendVerificationCodeAsync(userId, verificationCodeModel.Address, addressType, code, cancellationToken);

            return notificationSent ? SendVerificationCodeResult.Success() : SendVerificationCodeResult.NotificationOrderFailed();
        }

        /// <inheritdoc/>
        /// This method might be deleted at a later time when all callers have migrated to using SendVerificationCodeAsync, which includes cooldown logic and resend functionality.
        public async Task<SendVerificationStatus> ResendVerificationCodeAsync(int userId, string address, AddressType addressType, CancellationToken cancellationToken)
        {
            var formattedAddress = VerificationCode.FormatAddress(address);

            var existingCode = await _addressVerificationRepository.GetVerificationCodeAsync(userId, addressType, formattedAddress, cancellationToken);
            if (existingCode is null)
            {
                _telemetry.RecordVerificationResendCodeNotFound(addressType);
                return SendVerificationStatus.CodeNotFound;
            }

            var sendResult = await SendVerificationCodeAsync(userId, formattedAddress, addressType, cancellationToken);
            return sendResult.Status;
        }

        private bool IsInCooldown(VerificationCode existingCode, out double secondsWaited)
        {
            secondsWaited = (DateTime.UtcNow - existingCode.Created).TotalSeconds;
            RecordResendPatienceTelemetry(existingCode.AddressType, secondsWaited);

            var isExistingCodeInCooldown = secondsWaited < _resendCoolDownSeconds;

            return isExistingCodeInCooldown;
        }

        private void RecordResendPatienceTelemetry(AddressType addressType, double secondsWaited)
        {
            _telemetry.RecordResendPatience(secondsWaited, addressType);
        }
    }
}
