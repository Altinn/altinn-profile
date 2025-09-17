namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing an update to a professional notification address.
    /// </summary>
    /// <param name="UserId">The user ID associated with the update.</param>
    /// <param name="PartyUuid">The unique identifier of the party.</param>
    /// <param name="CreationTimestamp">The timestamp when the event was created.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    /// <param name="EmailAddress">The emailAddress of the notificationSettings</param>
    /// <param name="PhoneNumber">The phoneNumber of the notificationSettings</param>
    /// <param name="ResourceIds">Optional, the selected resourceIds of the notificationSettings</param>
    public record NotificationSettingsUpdatedEvent(
        int UserId,
        Guid PartyUuid,
        DateTime CreationTimestamp,
        DateTime EventTimestamp,
        string? EmailAddress,
        string? PhoneNumber,
        string[]? ResourceIds);
}
