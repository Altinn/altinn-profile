using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.NotificationSettings;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class NotificationSettingsUpdatedHandlerTests
{
    [Fact]
    public async Task Handle_UpdateA2True_CallsUpdateNotificationSettings_WithCorrectRequest()
    {
        // Arrange
        var mockClient = new Mock<IUserNotificationSettingsClient>();
        NotificationSettingsChangedRequest capturedRequest = null;
        mockClient.Setup(c => c.UpdateNotificationSettings(It.IsAny<NotificationSettingsChangedRequest>()))
            .Callback<NotificationSettingsChangedRequest>(req => capturedRequest = req)
            .Returns(Task.CompletedTask);

        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2NotificationSettings = true });

        var handler = new NotificationSettingsUpdatedHandler(mockClient.Object, settingsMock.Object);

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

        // Assert
        mockClient.Verify(c => c.UpdateNotificationSettings(It.IsAny<NotificationSettingsChangedRequest>()), Times.Once);
        Assert.NotNull(capturedRequest);
        Assert.Equal(evt.UserId, capturedRequest.UserId);
        Assert.Equal("update", capturedRequest.ChangeType);
        Assert.Equal(evt.PartyUuid, capturedRequest.PartyUuid);
        Assert.Equal(evt.EventTimestamp, capturedRequest.ChangeDateTime, TimeSpan.FromSeconds(1));
        Assert.Equal(evt.EmailAddress, capturedRequest.Email);
        Assert.Equal(evt.PhoneNumber, capturedRequest.PhoneNumber);
        Assert.Equal(evt.ResourceIds, capturedRequest.ServiceNotificationOptions);
    }

    [Fact]
    public async Task Handle_UpdateA2False_DoesNotCallUpdateNotificationSettings()
    {
        // Arrange
        var mockClient = new Mock<IUserNotificationSettingsClient>();
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2NotificationSettings = false });

        var handler = new NotificationSettingsUpdatedHandler(mockClient.Object, settingsMock.Object);

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

        // Assert
        mockClient.Verify(c => c.UpdateNotificationSettings(It.IsAny<NotificationSettingsChangedRequest>()), Times.Never);
    }
}
