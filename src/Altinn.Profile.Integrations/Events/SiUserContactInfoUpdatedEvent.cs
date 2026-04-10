namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing an update of a private notification address for a self-identified user.
    /// </summary>
    /// <param name="UserId">The user ID that updated the address.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    /// <param name="EmailAddress">The emailAddress of the notificationSettings</param>
    /// <param name="PhoneNumber">The phoneNumber of the notificationSettings</param>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public record SiUserContactInfoUpdatedEvent(
        int UserId,
        DateTime EventTimestamp,
        string? EmailAddress,
        string? PhoneNumber);
}
