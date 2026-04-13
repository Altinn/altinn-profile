using System.Text.Json.Serialization;

namespace Altinn.Profile.Core.AddressVerifications.Models
{
    /// <summary>
    /// Specifies the type of address verification.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VerificationType
    {
        /// <summary>
        /// Legacy verification type.
        /// </summary>
        Legacy,

        /// <summary>
        /// Verification type for addresses that are explicitly verified by the user.
        /// </summary>
        Verified,

        /// <summary>
        /// Verification type for addresses that are unverified.
        /// </summary>
        Unverified
    }
}
