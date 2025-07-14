using Altinn.Profile.Integrations.Events;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where a party has been removed from a user's favorites.
/// </summary>
public static class FavoriteRemovedEventHandler
{
    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public static async Task Handle(FavoriteRemovedEvent changeEvent)
    {
        Console.WriteLine("ChangeInFavoritesEventHandler.Handle: changeEvent = {0}", changeEvent.ToString());
        await Task.CompletedTask;
    }
}
