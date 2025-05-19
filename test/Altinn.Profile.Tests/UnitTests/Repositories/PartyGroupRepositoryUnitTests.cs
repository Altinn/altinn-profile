using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Models;

namespace Altinn.Profile.Tests.UnitTests.Repositories
{
    public class PartyGroupRepositoryUnitTests
    {
        private ProfileDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ProfileDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ProfileDbContext(options);
        }

        private PartyGroupRepository CreateRepository(ProfileDbContext context) =>
            new PartyGroupRepository(context);

        [Fact]
        public async Task AddPartyGroupAsync_ValidGroup_SavesToDatabase()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var repository = CreateRepository(context);
            var group = new PartyGroup { Name = "Test Group" };

            // Act
            await repository.AddPartyGroupAsync(group);

            // Assert
            var saved = await context.PartyGroups.FindAsync(group.Id);
            Assert.NotNull(saved);
            Assert.Equal("Test Group", saved.Name);
        }

        [Fact]
        public async Task AddPartyGroupAsync_NullGroup_ThrowsArgumentNullException()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var repository = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddPartyGroupAsync(null));
        }

        [Fact]
        public async Task UpdatePartyGroupAsync_NullGroup_ThrowsArgumentNullException()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var repository = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdatePartyGroupAsync(null));
        }

        [Fact]
        public async Task GetPartyGroupByIdAsync_ExistingId_ReturnsGroup()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var group = new PartyGroup { Name = "Existing" };
            context.PartyGroups.Add(group);
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetPartyGroupByIdAsync(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Existing", result.Name);
        }

        [Fact]
        public async Task GetPartyGroupByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetPartyGroupByIdAsync(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPartyGroupsAsync_MultipleGroups_ReturnsAll()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            context.PartyGroups.AddRange(
                new PartyGroup { Name = "Group A" },
                new PartyGroup { Name = "Group B" }
            );
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            var groups = await repository.GetAllPartyGroupsAsync();

            // Assert
            Assert.Equal(2, groups.Count);
            Assert.Contains(groups, g => g.Name == "Group A");
            Assert.Contains(groups, g => g.Name == "Group B");
        }

        [Fact]
        public async Task UpdatePartyGroupAsync_ExistingGroup_ChangesName()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var group = new PartyGroup { Name = "Original" };
            context.PartyGroups.Add(group);
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            group.Name = "Updated";
            await repository.UpdatePartyGroupAsync(group);

            // Assert
            var updated = await context.PartyGroups.FindAsync(group.Id);
            Assert.Equal("Updated", updated.Name);
        }

        [Fact]
        public async Task DeletePartyGroupAsync_ExistingId_RemovesGroup()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var group = new PartyGroup { Name = "ToDelete" };
            context.PartyGroups.Add(group);
            await context.SaveChangesAsync();
            var repository = CreateRepository(context);

            // Act
            await repository.DeletePartyGroupAsync(group.Id);

            // Assert
            var deleted = await context.PartyGroups.FindAsync(group.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeletePartyGroupAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            await using var context = CreateInMemoryContext();
            var repository = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.DeletePartyGroupAsync(-1));
        }
    }
}