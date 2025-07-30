namespace Altinn.Profile.Integrations.Events;

/// <summary>
/// Represents an event that notifies of a removal of a party from a user's favorites.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose favorites has changed. Must be a positive integer.</param>
/// <param name="PartyUuid">The unique identifier of the party that was removed from the user's favorites.</param>
/// <param name="CreationTimestamp">Creation timestamp for the favorite</param>
/// <param name="DeletionTimestamp">Deletion timestamp for the favorite</param>
public record FavoriteRemovedEvent(
    int UserId,
    Guid PartyUuid,
    DateTime CreationTimestamp,
    DateTime DeletionTimestamp);
