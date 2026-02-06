#nullable enable

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Request model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public class NotificationSettingsRequest : ProfessionalNotificationAddress
    {
        /// <summary>
        /// A feature flag to indicate whether a verification code should be generated and sent to the provided email address or phone number. This is used to verify that the user has access to the provided contact information before it is saved as a notification address. If set to true, a verification code will be generated and sent, and the user will need to verify the code before the notification address is considered valid.
        /// </summary>
        public bool? GenerateVerificationCode { get; init; } = false;
    }
}
