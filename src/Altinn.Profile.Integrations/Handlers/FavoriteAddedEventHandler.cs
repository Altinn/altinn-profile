using Altinn.Profile.Integrations.Events;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where a party has been added to a user's favorites.
/// </summary>
public static class FavoriteAddedEventHandler
{
    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public static async Task Handle(FavoriteAddedEvent changeEvent)
    {
        Console.WriteLine("ChangeInFavoritesEventHandler.Handle: changeEvent = {0}", changeEvent.ToString());
        await Task.CompletedTask;
    }
}
