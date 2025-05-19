// Note: This test file relies on xUnit as the test framework and Moq as the mocking library.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Models.PartyGroups;

namespace Altinn.Profile.Tests.Profile.Core.PartyGroups
{
    public class PartyGroupServiceTests
    {
        private readonly Mock<IPartyGroupRepository> _mockRepository;
        private readonly PartyGroupService _service;

        public PartyGroupServiceTests()
        {
            _mockRepository = new Mock<IPartyGroupRepository>();
            _service = new PartyGroupService(_mockRepository.Object);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PartyGroupService(null));
        }

        [Fact]
        public async Task GetFavorites_ValidUserId_ReturnsFavoriteIds()
        {
            // Arrange
            int userId = 123;
            var expected = new[] { 1, 2, 3 };
            _mockRepository.Setup(r => r.GetFavoritesAsync(userId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(expected);

            // Act
            int[] actual = await _service.GetFavorites(userId, CancellationToken.None);

            // Assert
            Assert.Equal(expected, actual);
            _mockRepository.Verify(r => r.GetFavoritesAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetFavorites_RepositoryThrows_PropagatesException()
        {
            // Arrange
            int userId = 999;
            _mockRepository.Setup(r => r.GetFavoritesAsync(userId, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new InvalidOperationException());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetFavorites(userId, CancellationToken.None));
        }

        [Fact]
        public async Task AddFavoriteAsync_ValidInputs_CallsRepository()
        {
            // Arrange
            int userId = 10, groupId = 20;
            _mockRepository.Setup(r => r.AddFavoriteAsync(userId, groupId))
                           .Returns(Task.CompletedTask);

            // Act
            await _service.AddFavoriteAsync(userId, groupId);

            // Assert
            _mockRepository.Verify(r => r.AddFavoriteAsync(userId, groupId), Times.Once);
        }

        [Theory]
        [InlineData(-1, 20)]
        [InlineData(10, -5)]
        public async Task AddFavoriteAsync_InvalidInputs_ThrowsArgumentException(int userId, int groupId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddFavoriteAsync(userId, groupId));
        }

        [Fact]
        public async Task RemoveFavoriteAsync_ValidInputs_CallsRepository()
        {
            // Arrange
            int userId = 15, groupId = 25;
            _mockRepository.Setup(r => r.RemoveFavoriteAsync(userId, groupId))
                           .Returns(Task.CompletedTask);

            // Act
            await _service.RemoveFavoriteAsync(userId, groupId);

            // Assert
            _mockRepository.Verify(r => r.RemoveFavoriteAsync(userId, groupId), Times.Once);
        }

        [Fact]
        public async Task RemoveFavoriteAsync_Nonexistent_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 99, groupId = 100;
            _mockRepository.Setup(r => r.RemoveFavoriteAsync(userId, groupId))
                           .ThrowsAsync(new KeyNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RemoveFavoriteAsync(userId, groupId));
        }
    }
}