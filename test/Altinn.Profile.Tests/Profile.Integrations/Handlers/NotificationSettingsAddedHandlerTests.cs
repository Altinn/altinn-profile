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

public class NotificationSettingsAddedHandlerTests
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
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2 = true });

        var handler = new NotificationSettingsAddedHandler(mockClient.Object, settingsMock.Object);

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
        mockClient.Verify(c => c.UpdateNotificationSettings(It.IsAny<NotificationSettingsChangedRequest>()), Times.Once);
        Assert.NotNull(capturedRequest);
        Assert.Equal(evt.UserId, capturedRequest.UserId);
        Assert.Equal("insert", capturedRequest.ChangeType);
        Assert.Equal(evt.PartyUuid, capturedRequest.PartyUuid);
        Assert.Equal(evt.EventTimestamp, capturedRequest.ChangeDateTime, TimeSpan.FromSeconds(1));
        Assert.Equal(evt.EmailAddress, capturedRequest.Email);
        Assert.Equal(evt.PhoneNumber, capturedRequest.PhoneNumber);
        Assert.Equal(evt.EventTimestamp, capturedRequest.LastModified, TimeSpan.FromSeconds(1));
        Assert.Equal(evt.ResourceIds, capturedRequest.ServiceNotificationOptions);
    }

    [Fact]
    public async Task Handle_UpdateA2False_DoesNotCallUpdateNotificationSettings()
    {
        // Arrange
        var mockClient = new Mock<IUserNotificationSettingsClient>();
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2 = false });

        var handler = new NotificationSettingsAddedHandler(mockClient.Object, settingsMock.Object);

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
        mockClient.Verify(c => c.UpdateNotificationSettings(It.IsAny<NotificationSettingsChangedRequest>()), Times.Never);
    }
}
