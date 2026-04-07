namespace Altinn.Profile.Core.AddressVerifications.Models
{
    /// <summary>
    /// Represents the outcome of sending a verification code, including resend/cooldown semantics.
    /// </summary>
    public sealed record SendVerificationCodeResult
    {
        /// <summary>
        /// Gets the send attempt result.
        /// </summary>
        public required SendVerificationStatus Status { get; init; }

        /// <summary>
        /// Gets the cooldown in seconds used for rate-limiting feedback.
        /// </summary>
        public required int Cooldown { get; init; }

        /// <summary>
        /// Creates a result indicating that the address has already been verified.
        /// </summary>
        /// <returns>A <see cref="SendVerificationCodeResult"/> with status <see cref="SendVerificationStatus.AddressAlreadyVerified"/>.</returns>
        public static SendVerificationCodeResult AlreadyVerified()
        {
            return new SendVerificationCodeResult
            {
                Status = SendVerificationStatus.AddressAlreadyVerified,
                Cooldown = 0,
            };
        }

        /// <summary>
        /// Creates a result indicating that a cooldown period is active before another code can be sent.
        /// </summary>
        /// <param name="cooldown">The cooldown period in seconds before a new verification code can be sent.</param>
        /// <returns>A <see cref="SendVerificationCodeResult"/> with status <see cref="SendVerificationStatus.CodeCooldown"/>.</returns>
        public static SendVerificationCodeResult CoolDown(int cooldown)
        {
            return new SendVerificationCodeResult
            {
                Status = SendVerificationStatus.CodeCooldown,
                Cooldown = cooldown,
            };
        }

        /// <summary>
        /// Creates a result indicating that a verification code was successfully sent.
        /// </summary>
        /// <returns>A <see cref="SendVerificationCodeResult"/> with status <see cref="SendVerificationStatus.Success"/>.</returns>
        public static SendVerificationCodeResult Success()
        {
            return new SendVerificationCodeResult
            {
                Status = SendVerificationStatus.Success,
                Cooldown = 0,
            };
        }

        /// <summary>
        /// Creates a result indicating that the notification order failed.
        /// </summary>
        /// <returns>A <see cref="SendVerificationCodeResult"/> with status <see cref="SendVerificationStatus.NotificationOrderFailed"/>.</returns>
        public static SendVerificationCodeResult NotificationOrderFailed()
        {
            return new SendVerificationCodeResult
            {
                Status = SendVerificationStatus.NotificationOrderFailed,
                Cooldown = 0,
            };
        }
    }
}
