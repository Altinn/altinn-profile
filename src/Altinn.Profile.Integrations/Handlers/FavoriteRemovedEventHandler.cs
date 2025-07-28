using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where a party has been removed from a user's favorites.
/// </summary>
/// <param name="client">The favorites client</param>
public class FavoriteRemovedEventHandler(IUserFavoriteClient client)
{
    private readonly IUserFavoriteClient _userFavoriteClient = client;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(FavoriteRemovedEvent changeEvent)
    {
        Console.WriteLine("ChangeInFavoritesEventHandler.Handle: changeEvent = {0}", changeEvent.ToString());

        var request = new FavoriteChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = "delete",
            PartyUuid = changeEvent.PartyUuid,
            ChangeDateTime = DateTime.UtcNow // Use current time for removal
        };

        // Using SBLBridge to update favorites in A2
        await _userFavoriteClient.UpdateFavorites(request);
        await Task.CompletedTask;
    }
}
