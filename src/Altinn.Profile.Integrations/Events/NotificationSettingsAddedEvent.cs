namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing the creation of a new professional notification address.
    /// </summary>
    /// <param name="UserId">The user ID that added the address.</param>
    /// <param name="PartyUuid">The unique identifier of the party.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    /// <param name="EmailAddress">The emailAddress of the notificationSettings</param>
    /// <param name="PhoneNumber">The phoneNumber of the notificationSettings</param>
    /// <param name="ResourceIds">Optional, the selected resourceIds of the notificationSettings</param>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public record NotificationSettingsAddedEvent(
        int UserId,
        Guid PartyUuid,
        DateTime EventTimestamp,
        string? EmailAddress,
        string? PhoneNumber,
        string[]? ResourceIds);
}
