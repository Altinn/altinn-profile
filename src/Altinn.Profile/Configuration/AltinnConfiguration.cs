namespace Altinn.Profile.Configuration
{
    /// <summary>
    /// Configuration settings for Altinn Profile
    /// </summary>
    public class AltinnConfiguration
    {
        /// <summary>
        /// The number of days a user will not be prompted to confirm their unit profile after choosing to ignore the confirmation.
        /// </summary>
        public int IgnoreUnitProfileConfirmationDays { get; set; }

        /// <summary>
        /// The number of days before personal entity consent expires that a reminder should be sent to the user.
        /// Called PersonalEntityConsentValidationReminderDays in Altinn2
        /// </summary>
        public int ValidationReminderDays { get; set; }
    }
}
