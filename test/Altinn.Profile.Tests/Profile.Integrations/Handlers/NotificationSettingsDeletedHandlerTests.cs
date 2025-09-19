using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class NotificationSettingsDeletedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2False_CompletesWithoutException()
    {
        // Arrange
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = false });
        var handler = new NotificationSettingsDeletedHandler(settings);
        var evt = new NotificationSettingsDeletedEvent(
            UserId: 2022,
            PartyUuid: Guid.NewGuid(),
            CreationTimestamp: DateTime.UtcNow,
            EventTimestamp: DateTime.UtcNow);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => handler.Handle(evt));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_UpdateA2True_CompletesWithoutException()
    {
        // Arrange
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = true });
        var handler = new NotificationSettingsDeletedHandler(settings);
        var evt = new NotificationSettingsDeletedEvent(
            UserId: 2023,
            PartyUuid: Guid.NewGuid(),
            EventTimestamp: DateTime.UtcNow,
            CreationTimestamp: DateTime.UtcNow);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => handler.Handle(evt));
        Assert.Null(exception);
    }
}
