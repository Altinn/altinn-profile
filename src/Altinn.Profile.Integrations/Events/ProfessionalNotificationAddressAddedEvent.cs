namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing the creation of a new professional notification address.
    /// </summary>
    /// <param name="UserId">The user ID that added the address.</param>
    /// <param name="PartyUuid">The unique identifier of the party.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    public record ProfessionalNotificationAddressAddedEvent(
        int UserId,
        Guid PartyUuid,
        DateTime EventTimestamp);
}
