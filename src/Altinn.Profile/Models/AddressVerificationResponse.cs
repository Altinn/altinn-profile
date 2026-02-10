using System.Text.Json.Serialization;

using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a response to an address verification request, indicating whether the provided verification code was correct and the address was successfully verified or not.
    /// </summary>
    public class AddressVerificationResponse
    {
        /// <summary>
        /// A boolean indicating wether the provided verification code was correct and the address was successfully verified or not.
        /// </summary>
        public bool Success { get; init; }
    }
}
