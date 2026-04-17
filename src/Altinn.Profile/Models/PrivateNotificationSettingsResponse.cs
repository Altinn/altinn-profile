namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public class PrivateNotificationSettingsResponse
    {
        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        /// <example>+4798765432</example>
        public string Value { get; set; }
    }
}
