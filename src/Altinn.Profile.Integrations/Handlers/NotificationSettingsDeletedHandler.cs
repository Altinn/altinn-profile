using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.NotificationSettings;

using Microsoft.Extensions.Options;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where the user deletes their notification settings for a party.
/// </summary>
/// <remarks>
/// Constructor for NotificationSettingsDeletedHandler
/// </remarks>
/// <param name="userNotificationSettingsClient">The notification settings client</param>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
public class NotificationSettingsDeletedHandler(IUserNotificationSettingsClient userNotificationSettingsClient, IOptions<SblBridgeSettings> settings)
{
    private readonly bool _updateA2 = settings.Value.UpdateA2NotificationSettings;
    private readonly IUserNotificationSettingsClient _userNotificationSettingsClient = userNotificationSettingsClient;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(NotificationSettingsDeletedEvent changeEvent)
    {
        if (!_updateA2)
        {
            return;
        }

        var request = new NotificationSettingsChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = ChangeType.Delete,
            PartyUuid = changeEvent.PartyUuid,
            ChangeDateTime = changeEvent.EventTimestamp,
        };

        // Using SBLBridge to update notification settings (ReporteeEndpoints) in A2
        await _userNotificationSettingsClient.UpdateNotificationSettings(request);
    }
}
