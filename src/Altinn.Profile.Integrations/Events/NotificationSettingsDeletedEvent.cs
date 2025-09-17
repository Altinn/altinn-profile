namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing an deletion of a professional notification address.
    /// </summary>
    /// <param name="UserId">The user ID that deleted their address.</param>
    /// <param name="PartyUuid">The unique identifier of the party.</param>
    /// <param name="CreationTimestamp">The timestamp when the event was created.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    public record NotificationSettingsDeletedEvent(
        int UserId,
        Guid PartyUuid,
        DateTime CreationTimestamp,
        DateTime EventTimestamp);
}
