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
        /// The existing verification code is too new to be resent (less than 1 minute old).
        /// </summary>
        CodeTooNew,
    }
}
