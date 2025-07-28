using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
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

            var handler = new FavoriteAddedEventHandler(mockClient.Object);

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
        public async Task FavoriteRemovedEventHandler_CallsUpdateFavorites_WithCorrectRequest()
        {
            // Arrange
            var mockClient = new Mock<IUserFavoriteClient>();
            FavoriteChangedRequest capturedRequest = null;
            mockClient.Setup(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()))
                .Callback<FavoriteChangedRequest>(req => capturedRequest = req)
                .Returns(Task.CompletedTask);

            var handler = new FavoriteRemovedEventHandler(mockClient.Object);

            var evt = new FavoriteRemovedEvent(UserId: 99, PartyUuid: Guid.NewGuid(), DateTime.Today);

            // Act
            var before = DateTime.UtcNow;
            await handler.Handle(evt);
            var after = DateTime.UtcNow;

            // Assert
            mockClient.Verify(c => c.UpdateFavorites(It.IsAny<FavoriteChangedRequest>()), Times.Once);
            Assert.NotNull(capturedRequest);
            Assert.Equal(evt.UserId, capturedRequest.UserId);
            Assert.Equal("delete", capturedRequest.ChangeType);
            Assert.Equal(evt.PartyUuid, capturedRequest.PartyUuid);

            // ChangeDateTime should be between before and after
            Assert.True(capturedRequest.ChangeDateTime >= before && capturedRequest.ChangeDateTime <= after);
        }
    }
}
