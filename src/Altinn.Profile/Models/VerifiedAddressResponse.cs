using System.Text.Json.Serialization;

using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a verified address response model that contains information about a verified address, including the address value, its type (email or phone), and its verification status.
    /// </summary>
    public class VerifiedAddressResponse
    {
        /// <summary>
        /// Gets or sets the address that has been verified. This could be an email address or a phone number, depending on the type of address being verified.
        /// </summary>
        public required string Value { get; init; }

        /// <summary>
        /// Gets or sets the type of the verified address. This indicates whether the address is an email address or a phone number.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AddressType Type { get; init; }

        /// <summary>
        /// Gets or sets the verification status of the address. This indicates whether the address has been successfully verified or not.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VerificationType VerificationStatus { get; init; }
    }
}
