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
        public string GenerateCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        /// <inheritdoc/>
        public VerificationCode CreateVerificationCode(int userId, string address, AddressType addressType, string verificationCode)
        {
            var verificationCodeHash = BCrypt.Net.BCrypt.HashPassword(verificationCode);
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
    }
}
