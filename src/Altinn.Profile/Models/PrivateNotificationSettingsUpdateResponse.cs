namespace Altinn.Profile.Models
{
    /// <summary>
    /// Response model for the private notification address for a self-identified user, used when updating the notification address.
    /// </summary>
    public class PrivateNotificationSettingsUpdateResponse
    {
        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        /// <example>+4798765432</example>
        public string Value { get; set; }
    }
}
