using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where a party has been added to a user's favorites.
/// </summary>
/// <remarks>
/// Constructor for FavoriteAddedEventHandler
/// </remarks>
/// <param name="client">The favorites client</param>
public class FavoriteAddedEventHandler(IUserFavoriteClient client)
{
    private readonly IUserFavoriteClient _userFavoriteClient = client;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(FavoriteAddedEvent changeEvent)
    {
        Console.WriteLine("ChangeInFavoritesEventHandler.Handle: changeEvent = {0}", changeEvent.ToString());

        var request = new FavoriteChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = "insert",
            PartyUuid = changeEvent.PartyUuid,
            ChangeDateTime = changeEvent.RegistrationTimestamp
        };

        // Using SBLBridge to update favorites in A2
        await _userFavoriteClient.UpdateFavorites(request);

        await Task.CompletedTask;
    }
}
