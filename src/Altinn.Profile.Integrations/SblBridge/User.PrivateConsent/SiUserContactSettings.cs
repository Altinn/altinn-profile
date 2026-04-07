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
        public string? PhoneNumber { get; init; }

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
    }
}
