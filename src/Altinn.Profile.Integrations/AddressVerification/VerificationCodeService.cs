using System.Security.Cryptography;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Integrations.AddressVerification
{
    /// <summary>
    /// Service for generating and managing verification codes for address verification.
    /// </summary>
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly int _expiryTimeInMinutes = 15;

        /// <inheritdoc/>
        public string GenerateRawCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        /// <summary>
        /// Hashes the provided verification code using BCrypt.
        /// </summary>
        /// <param name="code">The verification code to hash.</param>
        /// <returns>The hashed verification code.</returns>
        public static string HashCode(string code)
        {
            return BCrypt.Net.BCrypt.HashPassword(code);
        }

        /// <inheritdoc/>
        public VerificationCode CreateVerificationCode(int userId, string address, AddressType addressType, string verificationCode)
        {
            var verificationCodeHash = HashCode(verificationCode);
            var verificationCodeModel = new VerificationCode
            {
                UserId = userId,
                Address = address,
                AddressType = addressType,
                VerificationCodeHash = verificationCodeHash,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_expiryTimeInMinutes),
            };

            return verificationCodeModel;
        }

        /// <inheritdoc/>
        public bool VerifyCode(string code, VerificationCode verificationCode)
        {
            if (string.IsNullOrEmpty(verificationCode.VerificationCodeHash))
            {
                return false;
            }

            if (verificationCode.Expires < DateTime.UtcNow)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(code, verificationCode.VerificationCodeHash);
        }
    }
}
