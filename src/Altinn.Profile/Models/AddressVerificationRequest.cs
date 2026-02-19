using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Gets or sets the verification code for the address.
        /// </summary>
        [Required]
        [JsonRequired]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$")]
        public required string VerificationCode { get; init; }
    }
}
