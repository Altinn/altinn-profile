using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class NotificationSettingsUpdatedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2False_CompletesWithoutException()
    {
        // Arrange
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = false });
        var handler = new NotificationSettingsUpdatedHandler(settings);
        var evt = new NotificationSettingsUpdatedEvent(
            UserId: 789,
            PartyUuid: Guid.NewGuid(),
            CreationTimestamp: DateTime.UtcNow,
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "updateduser@example.com",
            PhoneNumber: "+4711122233",
            ResourceIds: ["resourceX", "resourceY"]);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => handler.Handle(evt));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_UpdateA2True_CompletesWithoutException()
    {
        // Arrange
        var settings = Options.Create(new SblBridgeSettings { UpdateA2 = true });
        var handler = new NotificationSettingsUpdatedHandler(settings);
        var evt = new NotificationSettingsUpdatedEvent(
            UserId: 1011,
            PartyUuid: Guid.NewGuid(),
            CreationTimestamp: DateTime.UtcNow,
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "anotherupdateduser@example.com",
            PhoneNumber: "+4799988877",
            ResourceIds: ["resourceZ", "resourceW"]);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => handler.Handle(evt));
        Assert.Null(exception);
    }
}
