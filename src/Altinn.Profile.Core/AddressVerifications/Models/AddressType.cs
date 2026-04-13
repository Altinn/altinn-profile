using System.Text.Json.Serialization;

namespace Altinn.Profile.Core.AddressVerifications.Models
{
    /// <summary>
    /// Specifies the type of address for verification.
    /// </summary>
    /// 
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AddressType
    {
        /// <summary>
        /// Email address type.
        /// </summary>
        Email,

        /// <summary>
        /// SMS address type.
        /// </summary>
        Sms
    }
}
