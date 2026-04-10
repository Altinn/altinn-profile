using System;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers;

public class SiUserContactInfoHandlerTests
{
    [Fact]
    public async Task UpdatedHandler_Handle_UpdateA2FlagFalse_DoesNotCallClient()
    {
        // Arrange
        var clientMock = new Mock<IPrivateConsentProfileClient>();
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2PrivateConsentProfile = false });

        var handler = new SiUserContactInfoUpdatedHandler(clientMock.Object, settingsMock.Object);

        var changeEvent = new SiUserContactInfoUpdatedEvent(
            UserId: 42,
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "user@example.com",
            PhoneNumber: "+4712345678");

        // Act
        await handler.Handle(changeEvent);

        // Assert
        clientMock.Verify(c => c.UpdatePrivateConsent(It.IsAny<PrivateConsentChangedRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdatedHandler_Handle_UpdateA2FlagTrue_CallsClientWithMappedRequest()
    {
        // Arrange
        var clientMock = new Mock<IPrivateConsentProfileClient>();
        PrivateConsentChangedRequest capturedRequest = null;
        clientMock
            .Setup(c => c.UpdatePrivateConsent(It.IsAny<PrivateConsentChangedRequest>()))
            .Callback<PrivateConsentChangedRequest>(req => capturedRequest = req)
            .Returns(Task.CompletedTask);

        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2PrivateConsentProfile = true });

        var handler = new SiUserContactInfoUpdatedHandler(clientMock.Object, settingsMock.Object);

        var changeEvent = new SiUserContactInfoUpdatedEvent(
            UserId: 123,
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "updated@example.com",
            PhoneNumber: "+4798765432");

        // Act
        await handler.Handle(changeEvent);

        // Assert
        clientMock.Verify(c => c.UpdatePrivateConsent(It.IsAny<PrivateConsentChangedRequest>()), Times.Once);
        Assert.NotNull(capturedRequest);
        Assert.Equal(changeEvent.UserId, capturedRequest.UserId);
        Assert.Equal(ChangeType.Update, capturedRequest.ChangeType);
        Assert.Equal(changeEvent.EventTimestamp, capturedRequest.ChangeDateTime);
        Assert.Equal(changeEvent.EmailAddress, capturedRequest.EmailAddress);
        Assert.Equal(changeEvent.PhoneNumber, capturedRequest.PhoneNumber);
    }

    [Fact]
    public async Task AddedHandler_Handle_UpdateA2FlagFalse_DoesNotCallClient()
    {
        // Arrange
        var clientMock = new Mock<IPrivateConsentProfileClient>();
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2PrivateConsentProfile = false });

        var handler = new SiUserContactInfoAddedHandler(clientMock.Object, settingsMock.Object);

        var changeEvent = new SiUserContactInfoAddedEvent(
            UserId: 42,
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "user@example.com",
            PhoneNumber: "+4712345678");

        // Act
        await handler.Handle(changeEvent);

        // Assert
        clientMock.Verify(c => c.UpdatePrivateConsent(It.IsAny<PrivateConsentChangedRequest>()), Times.Never);
    }

    [Fact]
    public async Task AddedHandler_Handle_UpdateA2FlagTrue_CallsClientWithMappedRequest()
    {
        // Arrange
        var clientMock = new Mock<IPrivateConsentProfileClient>();
        PrivateConsentChangedRequest capturedRequest = null;
        clientMock
            .Setup(c => c.UpdatePrivateConsent(It.IsAny<PrivateConsentChangedRequest>()))
            .Callback<PrivateConsentChangedRequest>(req => capturedRequest = req)
            .Returns(Task.CompletedTask);

        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2PrivateConsentProfile = true });

        var handler = new SiUserContactInfoAddedHandler(clientMock.Object, settingsMock.Object);

        var changeEvent = new SiUserContactInfoAddedEvent(
            UserId: 456,
            EventTimestamp: DateTime.UtcNow,
            EmailAddress: "new@example.com",
            PhoneNumber: "+4711223344");

        // Act
        await handler.Handle(changeEvent);

        // Assert
        clientMock.Verify(c => c.UpdatePrivateConsent(It.IsAny<PrivateConsentChangedRequest>()), Times.Once);
        Assert.NotNull(capturedRequest);
        Assert.Equal(changeEvent.UserId, capturedRequest.UserId);
        Assert.Equal(ChangeType.Insert, capturedRequest.ChangeType);
        Assert.Equal(changeEvent.EventTimestamp, capturedRequest.ChangeDateTime);
        Assert.Equal(changeEvent.EmailAddress, capturedRequest.EmailAddress);
        Assert.Equal(changeEvent.PhoneNumber, capturedRequest.PhoneNumber);
    }
}
