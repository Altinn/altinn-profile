using Altinn.Profile.Integrations.Events;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Represents an event that indicates a change in a user's favorites.
/// </summary>
public static class FavoriteAddedEventHandler
{
    /// <summary>
    /// Handle an event that indicates a change in a user's favorites.
    /// </summary>
    [Transactional]
    public static async Task Handle(FavoriteAddedEvent changeEvent)
    {
        Console.WriteLine("ChangeInFavoritesEventHandler.Handle: changeEvent = {0}", changeEvent.ToString());
        await Task.CompletedTask;
    }
}
