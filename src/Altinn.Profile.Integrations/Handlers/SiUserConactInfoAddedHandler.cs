using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Microsoft.Extensions.Options;

using Wolverine.Attributes;

namespace Altinn.Profile.Integrations.Handlers;

/// <summary>
/// Handler for the event where the user adds their private contact information (email and/or phone number). 
/// This handler will update the private consent profile in Altinn 2 via SBLBridge, to ensure that the user receives notifications according to their preferences.
/// </summary>
/// <remarks>
/// Can be removed when Altinn2 is decommissioned.
/// </remarks>
/// <param name="privateConsentProfileClient">The private consent profile client</param>
/// <param name="settings">Config to indicate if the handler should update Altinn 2</param>
public class SiUserConactInfoAddedHandler(IPrivateConsentProfileClient privateConsentProfileClient, IOptions<SblBridgeSettings> settings)
{
    private readonly bool _updateA2 = settings.Value.UpdateA2PrivateConsentProfile;
    private readonly IPrivateConsentProfileClient _privateConsentProfileClient = privateConsentProfileClient;

    /// <summary>
    /// Handles the event
    /// </summary>
    [Transactional]
    public async Task Handle(SiUserContactInfoAddedEvent changeEvent)
    {
        if (!_updateA2)
        {
            return;
        }

        var request = new PrivateConsentChangedRequest
        {
            UserId = changeEvent.UserId,
            ChangeType = ChangeType.Insert,
            ChangeDateTime = changeEvent.EventTimestamp,
            EmailAddress = changeEvent.EmailAddress,
            PhoneNumber = changeEvent.PhoneNumber,
        };

        // Using SBLBridge to update private consent profile in A2
        await _privateConsentProfileClient.UpdatePrivateConsent(request);
    }
}
