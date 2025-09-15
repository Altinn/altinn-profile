namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing an update to a professional notification address.
    /// </summary>
    /// <param name="UserId">The user ID associated with the update.</param>
    /// <param name="PartyUuid">The unique identifier of the party.</param>
    /// <param name="CreationTimestamp">The timestamp when the event was created.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    public record ProfessionalNotificationAddressUpdatedEvent(
        int UserId,
        Guid PartyUuid,
        DateTime CreationTimestamp,
        DateTime EventTimestamp);
}
