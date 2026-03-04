namespace Altinn.Profile.Core.AddressVerifications.Models
{
    /// <summary>
    /// Specifies the result of a resend verification code operation.
    /// </summary>
    public enum ResendVerificationResult
    {
        /// <summary>
        /// The verification code was successfully generated and sent.
        /// </summary>
        Success,

        /// <summary>
        /// No verification code was found for the specified user and address.
        /// </summary>
        CodeNotFound,

        /// <summary>
        /// An existing verification code was in the state of cooldown/timeout
        /// </summary>
        CodeCooldown,
    }
}
