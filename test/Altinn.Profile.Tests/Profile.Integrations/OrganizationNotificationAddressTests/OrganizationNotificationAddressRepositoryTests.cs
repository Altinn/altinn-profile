using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests;

/// <summary>
/// Contains unit tests for the <see cref="OrganizationNotificationAddressRepository"/> class.
/// </summary>
public class OrganizationNotificationAddressRepositoryTests : IDisposable
{
    private bool _isDisposed;
    private readonly ProfileDbContext _databaseContext;
    private readonly OrganizationNotificationAddressRepository _repository;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public OrganizationNotificationAddressRepositoryTests()
    {
        var databaseContextOptions = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(new ProfileDbContext(databaseContextOptions));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(databaseContextOptions));

        _repository = new OrganizationNotificationAddressRepository(_databaseContextFactory.Object, null);

        _databaseContext = _databaseContextFactory.Object.CreateDbContext();
    }

    private void SeedDatabase(List<OrganizationDE> organizations, List<NotificationAddressDE> notificationAddresses)
    {
        _databaseContext.NotificationAddresses.AddRange(notificationAddresses);
        _databaseContext.Organizations.AddRange((IEnumerable<OrganizationDE>)organizations);
        _databaseContext.SaveChanges();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _databaseContext.Database.EnsureDeleted();
                _databaseContext.Dispose();
            }

            _isDisposed = true;
        }
    }

    [Fact]
    public async Task GetOrganization_WhenFound_ReturnsWithNotificationAddresses()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        // Act
        var matchedOrg = await _repository.GetOrganizationDEAsync("123456789", CancellationToken.None);

        var expectedOrg = organizations
            .Find(p => p.RegistryOrganizationNumber == "123456789");

        // Assert
        Assert.NotNull(matchedOrg);
        Assert.NotEmpty(matchedOrg.NotificationAddresses);
        Assert.Equal(matchedOrg.NotificationAddresses.Count, expectedOrg.NotificationAddresses.Count);
        AssertRegisterProperties(expectedOrg, matchedOrg);
    }

    [Fact]
    public async Task GetOrganization_WhenGivenOrganizationDoesNotExist_ReturnsNull()
    {
        // Arrange - skip seeding the DB with data
        // Act
        var matchedOrg = await _repository.GetOrganizationDEAsync("111111111", CancellationToken.None);

        // Assert
        Assert.Null(matchedOrg);
    }

    [Fact]
    public async Task GetOrganizationNotificationAddressByPhoneNumber_WhenFound_ReturnsWithNotificationAddresses() 
    { 
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        // Act
        var result = await _repository.GetOrganizationNotificationAddressesByPhoneNumberAsync("12345678", CancellationToken.None);
        var list = result.ToList();

        // Assert
        Assert.Single(list);
        var returned = list.Single();
        Assert.Equal("123456789", returned.OrganizationNumber);

        Assert.NotNull(returned.NotificationAddresses);
        Assert.Equal(2, returned.NotificationAddresses.Count);
        Assert.All(returned.NotificationAddresses, na => Assert.True(na.IsSoftDeleted != true));
        Assert.Contains(returned.NotificationAddresses, na => na.AddressType == AddressType.Email && na.FullAddress == "test.email@test.no");
    }

    [Fact]
    public async Task GetOrganizationNotificationAddressByPhoneNumber_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);
        
        // Act
        var result = await _repository.GetOrganizationNotificationAddressesByPhoneNumberAsync("doesnotexist@test.com", CancellationToken.None);
        var list = result.ToList();
        
        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithProperData_ReturnsUpdatedRows()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1");

        // Act
        var numberOfUpdatedRows = await _repository.SyncNotificationAddressesAsync(changes);
        var updatedOrg1 = await _repository.GetOrganizationDEAsync("123456789", CancellationToken.None);
        var updatedOrg2 = await _repository.GetOrganizationDEAsync("920212345", CancellationToken.None);

        // Assert
        Assert.NotNull(updatedOrg1);
        Assert.Equal(4, updatedOrg1.NotificationAddresses.Count);
        Assert.NotNull(updatedOrg2);
        Assert.Single(updatedOrg2.NotificationAddresses);
        Assert.Equal(3, numberOfUpdatedRows);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithoutOrgNumber_ReturnZero()
    {
        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_6");

        // Act
        var numberOfUpdatedRows = await _repository.SyncNotificationAddressesAsync(changes);

        // Assert
        Assert.Equal(0, numberOfUpdatedRows);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithProperDataWithNullablePhonePrefix_ReturnsUpdatedRows()
    {
        // Arrange
        var changesIncludingMissingPhonePrefix = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2");

        // Act
        var numberOfUpdatedRows = await _repository.SyncNotificationAddressesAsync(changesIncludingMissingPhonePrefix);
        var updatedOrg1 = await _repository.GetOrganizationDEAsync("920254321", CancellationToken.None);
        var updatedOrg2 = await _repository.GetOrganizationDEAsync("920212345", CancellationToken.None);

        // Assert
        Assert.NotNull(updatedOrg1);
        Assert.Equal(2, updatedOrg1.NotificationAddresses.Count);
        Assert.NotNull(updatedOrg2);
        Assert.Equal(2, updatedOrg2.NotificationAddresses.Count);
        Assert.Equal(6, numberOfUpdatedRows);
        Assert.Null(updatedOrg1.NotificationAddresses.Last().Domain);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithDeleteData_ReturnsUpdatedRows()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var changeWithDelete = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_4");
        var orgToUpdate = "987654321";

        // Act
        var numberOfUpdatedAddresses = await _repository.SyncNotificationAddressesAsync(changeWithDelete);
        var updatedOrg = await _repository.GetOrganizationDEAsync(orgToUpdate, CancellationToken.None);

        // Assert
        Assert.Single(updatedOrg.NotificationAddresses);
        Assert.Equal(1, numberOfUpdatedAddresses);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithDeleteData_ReturnsNullWhenAlreadyDeleted()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_4");

        // Act - call delete twice
        await _repository.SyncNotificationAddressesAsync(changes);
        var numberOfUpdatedAddresses = await _repository.SyncNotificationAddressesAsync(changes);
        var updatedOrg = await _repository.GetOrganizationDEAsync("987654321", CancellationToken.None);

        // Assert
        Assert.Single(updatedOrg.NotificationAddresses);
        Assert.Equal(0, numberOfUpdatedAddresses);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenUpdatingExistingAddress_ReturnsUpdatedRows()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_5");
        var identifierForAddressToUpdate = "27ae0c8bea1f4f02a974c10429c32758";
        var addressToReplace = notificationAddresses.Find(address => address.RegistryID == identifierForAddressToUpdate);

        // Act
        var numberOfUpdatedAddresses = await _repository.SyncNotificationAddressesAsync(changes);

        // Assert
        var actualOrg = await _repository.GetOrganizationDEAsync("987654321", CancellationToken.None);
        Assert.Equal(2, actualOrg.NotificationAddresses.Count);

        var actualUpdatedAddress = actualOrg.NotificationAddresses.Find(address => address.RegistryID == identifierForAddressToUpdate);
        Assert.NotNull(actualUpdatedAddress);
        Assert.NotEqual(addressToReplace.Address, actualUpdatedAddress.Address);
        Assert.True(actualUpdatedAddress.HasRegistryAccepted);
        Assert.Equal(UpdateSource.KoFuVi, actualUpdatedAddress.UpdateSource);
        Assert.True(numberOfUpdatedAddresses > 0);
    }

    [Fact]
    public async Task GetSingleOrganization_WhenFound_ReturnsWithNotificationAddresses()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var orgNumberLookup = "123456789";

        var expectedOrg1 = organizations
            .Find(p => p.RegistryOrganizationNumber == "123456789");

        // Act
        var result = await _repository.GetOrganizationAsync(orgNumberLookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Organization>(result);
        Assert.NotEmpty(result.NotificationAddresses);
        Assert.Equal(result.NotificationAddresses.Count, expectedOrg1.NotificationAddresses.Count);
        Assert.Equal(result.OrganizationNumber, expectedOrg1.RegistryOrganizationNumber);
    }

    [Fact]
    public async Task GetOrganizations_WhenFound_ReturnsWithNotificationAddresses()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var orgNumberLookup = new List<string>() { "123456789", "987654321" };

        var expectedOrg1 = organizations
            .Find(p => p.RegistryOrganizationNumber == "123456789");

        // Act
        var result = await _repository.GetOrganizationsAsync(orgNumberLookup, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result);
        var matchedOrg1 = result.FirstOrDefault();
        Assert.IsType<Organization>(matchedOrg1);
        Assert.NotEmpty(matchedOrg1.NotificationAddresses);
        Assert.Equal(matchedOrg1.NotificationAddresses.Count, expectedOrg1.NotificationAddresses.Count);
        Assert.Equal(matchedOrg1.OrganizationNumber, expectedOrg1.RegistryOrganizationNumber);
    }

    [Fact]
    public async Task GetOrganizations_WhenFoundButAddressSoftDeleted_ReturnsWithoutNotificationAddresses()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var testOrgWithOnlySoftDeletedAddresses = new List<string>() { "999999999" };

        var expectedOrg1 = organizations
            .Find(p => p.RegistryOrganizationNumber == "999999999");

        // Act
        var result = await _repository.GetOrganizationsAsync(testOrgWithOnlySoftDeletedAddresses, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result);
        var matchedOrg1 = result.FirstOrDefault();
        Assert.IsType<Organization>(matchedOrg1);
        Assert.Empty(matchedOrg1.NotificationAddresses);
    }

    [Fact]
    public async Task GetOrganizations_WhenNoneFound_ReturnsEmptyList()
    {
        // Arrange
        var orgNumberLookup = new List<string>() { "000000000" };

        // Act
        var orgList = await _repository.GetOrganizationsAsync(orgNumberLookup, CancellationToken.None);

        // Assert
        Assert.Empty(orgList);
    }

    [Fact]
    public async Task CreateNotificationAddressAsync_WhenFirstAdded_ReturnsStoredNotificationAddress()
    {
        // Arrange
        var orgNumber = "000000000";

        // Act
        var na = await _repository.CreateNotificationAddressAsync(orgNumber, new NotificationAddress { AddressType = AddressType.Email, FullAddress = "test@test.com", Address = "test" }, "1");

        // Assert;
        Assert.IsType<NotificationAddress>(na);
        Assert.NotNull(na.RegistryID);
        Assert.NotEqual(default, na.NotificationAddressID);
    }

    [Fact]
    public async Task CreateNotificationAddressAsync_WhenFound_ReturnsWithAllNotificationAddresses()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var orgNumber = "123456789";

        // Act
        var na = await _repository.CreateNotificationAddressAsync(orgNumber, new NotificationAddress { AddressType = AddressType.Email, FullAddress = "test@test.com", Address = "test" }, "1");

        // Assert
        Assert.IsType<NotificationAddress>(na);
        Assert.NotNull(na.RegistryID);
        Assert.NotEqual(default, na.NotificationAddressID);
    }

    [Fact]
    public async Task UpdateNotificationAddressAsync_WhenFound_ReturnsUpdatedNotificationAddress()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var notificationAddressId = 1;

        var existingAddress = notificationAddresses
            .Find(p => p.NotificationAddressID == 1);

        // Act
        var updatedAddress = await _repository.UpdateNotificationAddressAsync(new NotificationAddress { AddressType = AddressType.Email, FullAddress = "something@new.com", Address = "test", NotificationAddressID = notificationAddressId }, "2");

        // Assert
        Assert.IsType<NotificationAddress>(updatedAddress);
        Assert.NotNull(updatedAddress.RegistryID);
        Assert.NotEqual(existingAddress.Address, updatedAddress.Address);
        Assert.NotEqual(existingAddress.Domain, updatedAddress.Domain);
        Assert.NotEqual(existingAddress.RegistryID, updatedAddress.RegistryID);
    }

    [Fact]
    public async Task UpdateNotificationAddressAsync_WhenFoundAndChangingAddressType_ReturnsUpdatedNotificationAddress()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var notificationAddressId = 1;

        var existingAddress = notificationAddresses
            .Find(p => p.NotificationAddressID == 1);

        // Act
        var updatedAddress = await _repository.UpdateNotificationAddressAsync(new NotificationAddress { AddressType = AddressType.SMS, FullAddress = "+4712345678", Address = "12345678", Domain = "+47", NotificationAddressID = notificationAddressId }, "2");

        // Assert
        Assert.IsType<NotificationAddress>(updatedAddress);
        Assert.NotNull(updatedAddress.RegistryID);
        Assert.NotEqual(existingAddress.Address, updatedAddress.Address);
        Assert.NotEqual(existingAddress.Domain, updatedAddress.Domain);
        Assert.NotEqual(existingAddress.RegistryID, updatedAddress.RegistryID);
        Assert.NotEqual(existingAddress.AddressType, updatedAddress.AddressType);
    }

    [Fact]
    public async Task DeleteNotificationAddressAsync_WhenFound_ReturnsSoftDeletedNotificationAddress()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var notificationAddressId = 1;

        // Act
        var updatedAddress = await _repository.DeleteNotificationAddressAsync(notificationAddressId);

        // Assert
        Assert.IsType<NotificationAddress>(updatedAddress);
        Assert.True(updatedAddress.IsSoftDeleted);
    }

    [Fact]
    public async Task RestoreNotificationAddress_WhenSoftDeleted_RestoresAndReturnsNotificationAddress()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        // Soft-delete an address first
        var notificationAddressId = notificationAddresses.First().NotificationAddressID;
        await _repository.DeleteNotificationAddressAsync(notificationAddressId);

        // Act
        var registryId = "restored-registry-id";
        var restored = await _repository.RestoreNotificationAddress(notificationAddressId, registryId);

        // Assert
        Assert.NotNull(restored);
        Assert.False(restored.IsSoftDeleted ?? true);
        Assert.Equal(registryId, restored.RegistryID);
    }

    [Fact]
    public async Task RestoreNotificationAddress_WhenNotSoftDeleted_StillReturnsNotificationAddressWithUpdatedRegistryId()
    {
        // Arrange
        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        SeedDatabase(organizations, notificationAddresses);

        var notificationAddressId = notificationAddresses.First().NotificationAddressID;
        var originalRegistryId = notificationAddresses.First().RegistryID;

        // Act
        var newRegistryId = "new-registry-id";
        var restored = await _repository.RestoreNotificationAddress(notificationAddressId, newRegistryId);

        // Assert
        Assert.NotNull(restored);
        Assert.False(restored.IsSoftDeleted ?? true);
        Assert.Equal(newRegistryId, restored.RegistryID);
        Assert.NotEqual(originalRegistryId, restored.RegistryID);
    }

    [Fact]
    public async Task RestoreNotificationAddress_WhenAddressDoesNotExist_ThrowsException()
    {
        // Arrange
        var nonExistentId = 999999;
        var registryId = "any-registry-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _repository.RestoreNotificationAddress(nonExistentId, registryId);
        });
    }

    private static void AssertRegisterProperties(OrganizationDE expected, OrganizationDE actual)
    {
        Assert.Equal(expected.RegistryOrganizationNumber, actual.RegistryOrganizationNumber);
        Assert.Equal(expected.RegistryOrganizationId, actual.RegistryOrganizationId);
    }
}
