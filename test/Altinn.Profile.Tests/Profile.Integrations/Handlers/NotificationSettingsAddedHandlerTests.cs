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

public class NotificationSettingsAddedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2False_DoesNothing()
    {
        // Arrange
        var clientMock = new Mock<IUserFavoriteClient>();
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = false });
        var handler = new NotificationSettingsAddedHandler(clientMock.Object, settings);
        var evt = new NotificationSettingsAddedEvent(
            UserId: 123,
            PartyUuid: Guid.NewGuid(),
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "user@example.com",
            PhoneNumber: "+4712345678",
            ResourceIds: new[] { "resource1", "resource2" });

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
        var handler = new NotificationSettingsAddedHandler(clientMock.Object, settings);
        var evt = new NotificationSettingsAddedEvent(
            UserId: 456,
            PartyUuid: Guid.NewGuid(),
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "anotheruser@example.com",
            PhoneNumber: "+4798765432",
            ResourceIds: new[] { "resourceA", "resourceB" });

        // Act
        await handler.Handle(evt);

        // Assert
        clientMock.VerifyNoOtherCalls();
    }
}
