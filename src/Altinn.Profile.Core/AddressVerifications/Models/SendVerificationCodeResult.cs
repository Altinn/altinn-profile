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
        /// Gets a value indicating whether notification delivery was accepted by the notification provider.
        /// </summary>
        public required bool NotificationSent { get; init; }
    }
}
