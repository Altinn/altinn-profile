using System.Text.Json.Serialization;

using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a request to verify an address, such as an email or phone number, using a verification code.
    /// </summary>
    public class AddressVerificationRequest
    {
        /// <summary>
        /// Gets or sets the address to verify, either an email or a phone number.
        /// </summary>
        [JsonRequired]
        public string Value { get; init; } = null!;

        /// <summary>
        /// Gets or sets the type of the address, either "email" or "sms".
        /// </summary>
        [JsonRequired]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AddressType Type { get; init; }

        /// <summary>
        /// Gets or sets the verification code for the address.
        /// </summary>
        [JsonRequired]
        public string VerificationCode { get; init; } = null!;
    }
}
