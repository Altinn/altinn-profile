using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Handlers
{
    public class FavoriteEventHandlersTests
    {
        [Fact]
        public async Task FavoriteAddedEventHandler_CallsUpdateFavorites_WithCorrectRequest()
        {
            // Arrange
            var mockClient = new Mock<IUserFavoriteClient>();
            FavoriteChangedRequest capturedRequest = null;
            mockClient.Setup(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()))
                .Callback<FavoriteChangedRequest>(req => capturedRequest = req)
                .Returns(Task.CompletedTask);
            var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2Favorites = true });

            var handler = new FavoriteAddedEventHandler(mockClient.Object, settingsMock.Object);

            var evt = new FavoriteAddedEvent(42, Guid.NewGuid(), DateTime.UtcNow);

            // Act
            await handler.Handle(evt);

            // Assert
            mockClient.Verify(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()), Times.Once);
            Assert.NotNull(capturedRequest);
            Assert.Equal(evt.UserId, capturedRequest.UserId);
            Assert.Equal("insert", capturedRequest.ChangeType);
            Assert.Equal(evt.PartyUuid, capturedRequest.PartyUuid);
            Assert.Equal(evt.RegistrationTimestamp, capturedRequest.ChangeDateTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task FavoriteAddedEventHandler_DoesNotCallUpdateFavorites_WhenUpdateA2IsFalse()
        {
            // Arrange
            var mockClient = new Mock<IUserFavoriteClient>();
            var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2Favorites = false });

            var handler = new FavoriteAddedEventHandler(mockClient.Object, settingsMock.Object);

            var evt = new FavoriteAddedEvent(42, Guid.NewGuid(), DateTime.UtcNow);

            // Act
            await handler.Handle(evt);

            // Assert
            mockClient.Verify(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()), Times.Never);
        }

        [Fact]
        public async Task FavoriteRemovedEventHandler_CallsUpdateFavorites_WithCorrectRequest()
        {
            // Arrange
            var mockClient = new Mock<IUserFavoriteClient>();
            FavoriteChangedRequest capturedRequest = null;
            mockClient.Setup(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()))
                .Callback<FavoriteChangedRequest>(req => capturedRequest = req)
                .Returns(Task.CompletedTask);

            var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2Favorites = true });

            var handler = new FavoriteRemovedEventHandler(mockClient.Object, settingsMock.Object);

            var evt = new FavoriteRemovedEvent(UserId: 99, PartyUuid: Guid.NewGuid(), DateTime.Today, DateTime.Now);

            // Act
            await handler.Handle(evt);

            // Assert
            mockClient.Verify(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()), Times.Once);
            Assert.NotNull(capturedRequest);
            Assert.Equal(evt.UserId, capturedRequest.UserId);
            Assert.Equal("delete", capturedRequest.ChangeType);
            Assert.Equal(evt.PartyUuid, capturedRequest.PartyUuid);
            Assert.Equal(evt.EventTimestamp, capturedRequest.ChangeDateTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task FavoriteRemovedEventHandler_DoesNotCallUpdateFavorites_WhenUpdateA2IsFalse()
        {
            // Arrange
            var mockClient = new Mock<IUserFavoriteClient>();
            var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { UpdateA2Favorites = false });

            var handler = new FavoriteRemovedEventHandler(mockClient.Object, settingsMock.Object);

            var evt = new FavoriteRemovedEvent(UserId: 99, PartyUuid: Guid.NewGuid(), DateTime.Today, DateTime.Now);

            // Act
            await handler.Handle(evt);

            // Assert
            mockClient.Verify(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()), Times.Never);
        }
    }
}
