using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Mappings;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;
using AutoMapper;
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
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new OrganizationMappingProfile());
        });
        var mapper = mapperConfig.CreateMapper();
        _repository = new OrganizationNotificationAddressRepository(_databaseContextFactory.Object, mapper);

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
        var matchedOrg = await _repository.GetOrganizationAsync("123456789");

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
        var matchedOrg = await _repository.GetOrganizationAsync("111111111");

        // Assert
        Assert.Null(matchedOrg);
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
        var updatedOrg1 = await _repository.GetOrganizationAsync("123456789");
        var updatedOrg2 = await _repository.GetOrganizationAsync("920212345");

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
        var updatedOrg1 = await _repository.GetOrganizationAsync("920254321");
        var updatedOrg2 = await _repository.GetOrganizationAsync("920212345");

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
        var updatedOrg = await _repository.GetOrganizationAsync(orgToUpdate);

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
        var updatedOrg = await _repository.GetOrganizationAsync("987654321");

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
        var actualOrg = await _repository.GetOrganizationAsync("987654321");
        Assert.Equal(2, actualOrg.NotificationAddresses.Count);

        var actualUpdatedAddress = actualOrg.NotificationAddresses.Find(address => address.RegistryID == identifierForAddressToUpdate);
        Assert.NotNull(actualUpdatedAddress);
        Assert.NotEqual(addressToReplace.Address, actualUpdatedAddress.Address);
        Assert.True(numberOfUpdatedAddresses > 0);
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
        var na = await _repository.CreateNotificationAddressAsync(orgNumber, new NotificationAddress { AddressType = AddressType.Email, FullAddress = "test@test.com", RegistryID = "1", Address = "test" });

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

        var expectedOrg1 = organizations
            .Find(p => p.RegistryOrganizationNumber == orgNumber);

        // Act
        var na = await _repository.CreateNotificationAddressAsync(orgNumber, new NotificationAddress { AddressType = AddressType.Email, FullAddress = "test@test.com", RegistryID = "1", Address = "test" });

        // Assert
        Assert.IsType<NotificationAddress>(na);
        Assert.NotNull(na.RegistryID);
        Assert.NotEqual(default, na.NotificationAddressID);
    }

    private static void AssertRegisterProperties(OrganizationDE expected, OrganizationDE actual)
    {
        Assert.Equal(expected.RegistryOrganizationNumber, actual.RegistryOrganizationNumber);
        Assert.Equal(expected.RegistryOrganizationId, actual.RegistryOrganizationId);
    }
}
