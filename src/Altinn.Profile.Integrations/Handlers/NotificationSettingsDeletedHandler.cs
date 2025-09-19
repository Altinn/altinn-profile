using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Microsoft.Extensions.Options;
using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where the user deletes their notification settings for a party.
/// </summary>
/// <remarks>
/// Constructor for NotificationSettingsDeletedHandler
/// </remarks>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
public class NotificationSettingsDeletedHandler(IOptions<SblBridgeSettings> settings)
{
    private readonly bool _updatea2 = settings.Value.UpdateA2;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(NotificationSettingsDeletedEvent changeEvent)
    {
        if (!_updatea2)
        {
            return;
        }

        await Task.CompletedTask;
    }
}
