namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing the creation of a private notification address for a self-identified user.
    /// </summary>
    /// <param name="UserId">The user ID that added the address.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    /// <param name="EmailAddress">The emailAddress</param>
    /// <param name="PhoneNumber">The phoneNumber</param>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public record SiUserContactInfoAddedEvent(
        int UserId,
        DateTime EventTimestamp,
        string? EmailAddress,
        string? PhoneNumber);
}
