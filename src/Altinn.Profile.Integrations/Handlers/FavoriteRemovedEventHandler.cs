using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Microsoft.Extensions.Options;
using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where a party has been removed from a user's favorites.
/// </summary>
/// <param name="client">The favorites client</param>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
/// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
public class FavoriteRemovedEventHandler(IUserFavoriteClient client, IOptions<SblBridgeSettings> settings)
{
    private readonly IUserFavoriteClient _userFavoriteClient = client;
    private readonly bool _updateA2 = settings.Value.UpdateA2Favorites;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(FavoriteRemovedEvent changeEvent)
    {
        if (!_updateA2)
        {
            return;
        }

        var request = new FavoriteChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = ChangeType.Delete,
            PartyUuid = changeEvent.PartyUuid,
            ChangeDateTime = changeEvent.EventTimestamp,
        };

        // Using SBLBridge to update favorites in A2
        await _userFavoriteClient.UpdateFavorites(request);
    }
}
