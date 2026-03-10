using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a verified address response model that contains information about a verified address, including the address value and its type (email or phone).
    /// </summary>
    public class VerifiedAddressResponse
    {
        /// <summary>
        /// The address that has been verified. This could be an email address or a phone number, depending on the type of address being verified.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Value { get; init; }

        /// <summary>
        /// The type of the verified address. This indicates whether the address is an email address or a phone number.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AddressType Type { get; init; }
    }
}
