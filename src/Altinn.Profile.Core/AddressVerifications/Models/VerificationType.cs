namespace Altinn.Profile.Core.AddressVerifications.Models
{
    /// <summary>
    /// Specifies the type of address verification.
    /// </summary>
    public enum VerificationType
    {
        /// <summary>
        /// Legacy verification type.
        /// </summary>
        Legacy,

        /// <summary>
        /// Verification type for addresses that is explicitly verified by the user.
        /// </summary>
        Explicit
    }
}
