using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.NotificationSettings;

using Microsoft.Extensions.Options;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where the user adds their notification settings for a party.
/// </summary>
/// <remarks>
/// Constructor for NotificationSettingsAddedHandler
/// </remarks>
/// <param name="userNotificationSettingsClient">The notification settings client</param>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
/// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
public class NotificationSettingsAddedHandler(IUserNotificationSettingsClient userNotificationSettingsClient, IOptions<SblBridgeSettings> settings)
{
    private readonly bool _updateA2 = settings.Value.UpdateA2NotificationSettings;
    private readonly IUserNotificationSettingsClient _userNotificationSettingsClient = userNotificationSettingsClient;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(NotificationSettingsAddedEvent changeEvent)
    {
        if (!_updateA2)
        {
            return;
        }

        var request = new NotificationSettingsChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = ChangeType.Insert,
            PartyUuid = changeEvent.PartyUuid,
            ChangeDateTime = changeEvent.EventTimestamp,
            Email = changeEvent.EmailAddress,
            PhoneNumber = changeEvent.PhoneNumber,
            ServiceNotificationOptions = changeEvent.ResourceIds,
        };

        // Using SBLBridge to update notification settings (ReporteeEndpoints) in A2
        await _userNotificationSettingsClient.UpdateNotificationSettings(request);
    }
}
