namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public class NotificationAddressResponse : NotificationAddressModel
    {
        /// <summary>
        /// An error message indicating why the notification address could not be updated
        /// Will be null when no error occurred.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
