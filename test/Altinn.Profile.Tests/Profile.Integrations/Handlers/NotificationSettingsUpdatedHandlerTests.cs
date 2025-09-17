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

public class NotificationSettingsUpdatedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2False_DoesNothing()
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

        // Act
        await handler.Handle(evt);
    }

    [Fact]
    public async Task Handle_UpdateA2True_CompletesSuccessfully()
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

        // Act
        await handler.Handle(evt);
    }
}
