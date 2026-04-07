namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// The personal contact information for a non-citizen user.
    /// </summary>
    public record UserContactInfo
    {
        /// <summary>
        /// The user identifier.
        /// </summary>
        public required int UserId { get; init; }

        /// <summary>
        /// UUID of the user
        /// </summary>
        public required Guid UserUuid { get; init; }

        /// <summary>
        /// The Username
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// The timestamp (with time-zone) for the event where the user's contact info was first registered in the system
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// The email address
        /// </summary>
        public required string EmailAddress { get; set; }

        /// <summary>
        /// The phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The timestamp (with time-zone) for the event where the user registered the mobile number
        /// </summary>
        public DateTime? PhoneNumberLastChanged { get; set; }
    }
}
