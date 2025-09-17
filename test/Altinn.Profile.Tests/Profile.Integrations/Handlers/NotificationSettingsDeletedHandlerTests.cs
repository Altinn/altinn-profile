using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class NotificationSettingsDeletedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2False_DoesNothing()
    {
        // Arrange
        var clientMock = new Mock<IUserFavoriteClient>();
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = false });
        var handler = new NotificationSettingsDeletedHandler(clientMock.Object, settings);
        var evt = new NotificationSettingsDeletedEvent(
            UserId: 2022,
            PartyUuid: Guid.NewGuid(),
            CreationTimestamp: DateTime.UtcNow,
            EventTimestamp: DateTime.UtcNow);

        // Act
        await handler.Handle(evt);

        // Assert
        clientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_UpdateA2True_CompletesSuccessfully()
    {
        // Arrange
        var clientMock = new Mock<IUserFavoriteClient>();
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = true });
        var handler = new NotificationSettingsDeletedHandler(clientMock.Object, settings);
        var evt = new NotificationSettingsDeletedEvent(
            UserId: 2023,
            PartyUuid: Guid.NewGuid(),
            EventTimestamp: DateTime.UtcNow,
            CreationTimestamp: DateTime.UtcNow);

        // Act
        await handler.Handle(evt);

        // Assert
        clientMock.VerifyNoOtherCalls();
    }
}
