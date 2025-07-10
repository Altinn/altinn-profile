namespace Altinn.Profile.Integrations.Events;

/// <summary>
/// Represents an event that indicates a change in a user's favorites.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose favorites has changed. Must be a positive integer.</param>
/// <param name="PartyUuid">The unique identifier of the party that were added or removed from the user's favorites.</param>
public record ChangeInFavoritesEvent(
    int UserId,
    Guid PartyUuid);
