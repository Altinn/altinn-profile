using System.Text.Json.Serialization;

namespace Altinn.Profile.Core.User.ProfileSettings
{
    /// <summary>
    /// Enumeration of supported languages for user profile settings.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Language>))]
    public enum Language
    {
        /// <summary>
        /// Norwegian Bokmål language.
        /// </summary>
        Nb,

        /// <summary>
        /// English language.
        /// </summary>
        En,

        /// <summary>
        /// Norwegian nynorsk language.
        /// </summary>
        Nn,
    }
}
