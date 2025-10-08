namespace Altinn.Profile.Integrations.Events;

/// <summary>
/// Represents an event that notifies of an addition of a party to a user's favorites.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose favorites has changed. Must be a positive integer.</param>
/// <param name="PartyUuid">The unique identifier of the party that was added to the user's favorites.</param>
/// <param name="RegistrationTimestamp">The timestamp for when the favorite-addition was registered</param>
/// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
public record FavoriteAddedEvent(
    int UserId,
    Guid PartyUuid,
    DateTime RegistrationTimestamp);
