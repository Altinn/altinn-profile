using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Core.User.ProfileSettings
{
    /// <summary>
    /// Enumeration of supported languages for user profile settings.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Language
    {
        /// <summary>
        /// Norwegian Bokmål language.
        /// </summary>
        [EnumMember(Value = "nb")]
        Nb,

        /// <summary>
        /// English language.
        /// </summary>
        [EnumMember(Value = "en")]
        En,

        /// <summary>
        /// Norwegian nynorsk language.
        /// </summary>
        [EnumMember(Value = "nn")]
        Nn,
    }
}
