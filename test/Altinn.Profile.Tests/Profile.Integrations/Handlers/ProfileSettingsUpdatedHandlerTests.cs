using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.ProfileSettings;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class ProfileSettingsUpdatedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2FlagFalse_DoesNotCallClient()
    {
        // Arrange
        var clientMock = new Mock<IProfileSettingsClient>();
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2ProfileSettings = false });

        var handler = new ProfileSettingsUpdatedHandler(clientMock.Object, settingsMock.Object);

        var changeEvent = new ProfileSettingsUpdatedEvent(42, DateTime.UtcNow, "en", false, Guid.NewGuid(), true, false, false, null);

        // Act
        await handler.Handle(changeEvent);

        // Assert
        clientMock.Verify(c => c.UpdatePortalSettings(It.IsAny<ProfileSettingsChangedRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateA2FlagTrue_CallsClientWithMappedRequest()
    {
        // Arrange
        var clientMock = new Mock<IProfileSettingsClient>();
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2ProfileSettings = true });

        var handler = new ProfileSettingsUpdatedHandler(clientMock.Object, settingsMock.Object);

        var changeEvent = new ProfileSettingsUpdatedEvent(123, DateTime.UtcNow, "nb", true, Guid.NewGuid(), true, false, true, DateTime.UtcNow.AddDays(-1));

        // Act
        await handler.Handle(changeEvent);

        // Assert - verify the request was built from the event and passed to the client
        clientMock.Verify(
            c => c.UpdatePortalSettings(It.Is<ProfileSettingsChangedRequest>(r =>
                r.UserId == changeEvent.UserId &&
                r.ChangeDateTime == changeEvent.EventTimestamp &&
                r.LanguageType == changeEvent.LanguageType &&
                r.DoNotPromptForParty == changeEvent.DoNotPromptForParty &&
                r.PreselectedPartyUuid == changeEvent.PreselectedPartyUuid &&
                r.ShowClientUnits == changeEvent.ShowClientUnits &&
                r.ShouldShowSubEntities == changeEvent.ShouldShowSubEntities &&
                r.ShouldShowDeletedEntities == changeEvent.ShouldShowDeletedEntities &&
                r.IgnoreUnitProfileDateTime == changeEvent.IgnoreUnitProfileDateTime)), 
            Times.Once);
    }
}
