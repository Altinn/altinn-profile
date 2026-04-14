using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Altinn.Profile.Integrations.SblBridge.User.PrivateConsent
{
    /// <summary>
    /// Represents user contact settings from SBL Bridge.
    /// </summary>
    public class SiUserContactSettings
    {
        /// <summary>
        /// Gets the user ID.
        /// </summary>
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the user UUID.
        /// </summary>
        public Guid UserUuid { get; set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Gets the email address.
        /// </summary>
        public string? EmailAddress { get; init; }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="SiUserContactSettings"/> object.
        /// </summary>
        /// <param name="data">The JSON string to deserialize.</param>
        /// <returns>A <see cref="SiUserContactSettings"/> object.</returns>
        public static SiUserContactSettings? Deserialize(string data)
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.DeserializeObject<SiUserContactSettings>(data, settings);
        }

        /// <summary>
        /// Formats a mobile number by standardizing its international prefix format.
        /// </summary>
        /// <param name="mobileNumber">The mobile number to format.</param>
        /// <returns>The formatted mobile number with standardized prefix, or the original number if it's already in the correct format or is a local 8-digit number.</returns>
        public static string FormatMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return mobileNumber;
            }

            // If the number starts with '00', replace it with '+'
            if (mobileNumber.StartsWith("00"))
            {
                mobileNumber = string.Concat("+", mobileNumber.AsSpan(2));
            }

            // Assume 8 digit numbers without international prefix are local and should be formatted with norwegian country code
            if (!mobileNumber.StartsWith('+') && !mobileNumber.StartsWith("00") && mobileNumber.Length == 8)
            {
                return string.Concat("+47", mobileNumber);
            }

            return mobileNumber;
        }
    }
}
