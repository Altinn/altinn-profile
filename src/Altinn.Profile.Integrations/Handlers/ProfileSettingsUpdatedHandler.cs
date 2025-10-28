using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.ProfileSettings;

using Microsoft.Extensions.Options;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where the user updates their profile settings.
/// </summary>
/// <param name="userProfileSettingsClient">The profile settings client</param>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
/// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
public class ProfileSettingsUpdatedHandler(IProfileSettingsClient userProfileSettingsClient, IOptions<SblBridgeSettings> settings)
{
    private readonly bool _updateA2 = settings.Value.UpdateA2PortalSettings;
    private readonly IProfileSettingsClient _userProfileSettingsClient = userProfileSettingsClient;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(ProfileSettingsUpdatedEvent changeEvent)
    {
        if (!_updateA2)
        {
            return;
        }

        var request = new ProfileSettingsChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = ChangeType.Update,
            ChangeDateTime = changeEvent.EventTimestamp,

            LanguageType = changeEvent.LanguageType,
            DoNotPromptForParty = changeEvent.DoNotPromptForParty,
            PreselectedPartyUuid = changeEvent.PreselectedPartyUuid,
            ShowClientUnits = changeEvent.ShowClientUnits,
            ShouldShowSubEntities = changeEvent.ShouldShowSubEntities,
            ShouldShowDeletedEntities = changeEvent.ShouldShowDeletedEntities,
            IgnoreUnitProfileDateTime = changeEvent.IgnoreUnitProfileDateTime,
        };

        // Using SBLBridge to update portal settings (preferences) in A2
        await _userProfileSettingsClient.UpdatePortalSettings(request);
    }
}
