using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Microsoft.Extensions.Options;
using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where a party has been added to a user's favorites.
/// </summary>
/// <remarks>
/// Constructor for FavoriteAddedEventHandler
/// </remarks>
/// <param name="client">The favorites client</param>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
public class FavoriteAddedEventHandler(IUserFavoriteClient client, IOptions<SblBridgeSettings> settings)
{
    private readonly IUserFavoriteClient _userFavoriteClient = client;
    private readonly bool _updatea2 = settings.Value.UpdateA2;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(FavoriteAddedEvent changeEvent)
    {
        if (!_updatea2)
        {
            return;
        }

        var request = new FavoriteChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = "insert",
            PartyUuid = changeEvent.PartyUuid,
            ChangeDateTime = changeEvent.RegistrationTimestamp
        };

        // Using SBLBridge to update favorites in A2
        await _userFavoriteClient.UpdateFavorites(request);
    }
}
