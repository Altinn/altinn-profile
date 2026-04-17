namespace Altinn.Profile.Models.AddressVerification
{
    /// <summary>
    /// Response model for address verification requests.
    /// </summary>
    public class AddressVerificationResponse
    {
        /// <summary>
        /// Gets the number of seconds before a new verification can be requested.
        /// </summary>
        public int CooldownSeconds { get; init; }

        /// <summary>
        /// Gets a value indicating whether a notification was sent during the verification process.
        /// </summary>
        public bool NotificationSent { get; init; }
    }
}
