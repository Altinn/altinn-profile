using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Provides methods for generating and retrieving verification codes for user address verification.
    /// </summary>
    public interface IVerificationCodeService
    {
        /// <summary>
        /// Generates a new verification code.
        /// </summary>
        /// <returns>A string containing the generated verification code.</returns>
        string GenerateCode();

        /// <summary>
        /// Creates a verification code object for a user and address.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="address">The address to verify.</param>
        /// <param name="addressType">The type of address (e.g., Email or Sms).</param>
        /// <param name="verificationCode">The verification code to validate.</param>
        /// <returns>A <see cref="VerificationCode"/> object if found; otherwise, null.</returns>
        VerificationCode CreateVerificationCode(int userId, string address, AddressType addressType, string verificationCode);
    }
}
