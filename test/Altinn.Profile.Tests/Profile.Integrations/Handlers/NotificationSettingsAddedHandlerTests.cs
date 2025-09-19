using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class NotificationSettingsAddedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2False_CompletesWithoutException()
    {
        // Arrange
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = false });
        var handler = new NotificationSettingsAddedHandler(settings);
        var evt = new NotificationSettingsAddedEvent(
            UserId: 123,
            PartyUuid: Guid.NewGuid(),
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "user@example.com",
            PhoneNumber: "+4712345678",
            ResourceIds: ["resource1", "resource2"]);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => handler.Handle(evt));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_UpdateA2True_CompletesWithoutException()
    {
        // Arrange
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = true });
        var handler = new NotificationSettingsAddedHandler(settings);
        var evt = new NotificationSettingsAddedEvent(
            UserId: 456,
            PartyUuid: Guid.NewGuid(),
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "anotheruser@example.com",
            PhoneNumber: "+4798765432",
            ResourceIds: ["resourceA", "resourceB"]);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => handler.Handle(evt));
        Assert.Null(exception);
    }
}
