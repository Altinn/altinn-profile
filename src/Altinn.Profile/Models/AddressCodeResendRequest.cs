using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a request to resend a verification code for a given address (email or phone number)
    /// </summary>
    public class AddressCodeResendRequest
    {
        /// <summary>
        /// Gets or sets the address to verify, either an email or a phone number.
        /// </summary>
        [Required]
        [JsonRequired]
        [StringLength(320)] // max email length per RFC 5321
        public required string Value { get; init; }

        /// <summary>
        /// Gets or sets the type of the address, either "email" or "sms".
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required AddressType? Type { get; init; }
    }
}
