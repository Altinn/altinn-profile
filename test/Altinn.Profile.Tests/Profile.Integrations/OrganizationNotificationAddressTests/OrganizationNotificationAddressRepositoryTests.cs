using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly List<NotificationAddressDE> _notificationAddressTestData;
    private readonly List<OrganizationDE> _organizationTestData;
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

        var (organizations, notificationAddresses) = OrganizationNotificationAddressTestData.GetNotificationAddresses();
        _notificationAddressTestData = notificationAddresses;
        _organizationTestData = organizations;

        _databaseContext = _databaseContextFactory.Object.CreateDbContext();
        _databaseContext.NotificationAddresses.AddRange(_notificationAddressTestData);
        _databaseContext.Organizations.AddRange((IEnumerable<OrganizationDE>)_organizationTestData);
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
        // Act
        var matchedOrg = await _repository.GetOrganizationAsync("123456789");

        var expectedOrg = _organizationTestData
            .Find(p => p.RegistryOrganizationNumber == "123456789");

        // Assert
        Assert.NotNull(matchedOrg);
        Assert.NotEmpty(matchedOrg.NotificationAddresses);
        Assert.Equal(matchedOrg.NotificationAddresses.Count, expectedOrg.NotificationAddresses.Count);
        AssertRegisterProperties(expectedOrg, matchedOrg);
    }

    [Fact]
    public async Task GetOrganization_WhenNoneFound_ReturnsNull()
    {
        // Act
        var matchedOrg = await _repository.GetOrganizationAsync("111111111");

        // Assert
        Assert.Null(matchedOrg);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithProperData_ReturnsUpdatedRows()
    {
        var matchedOrg1 = await _repository.GetOrganizationAsync("123456789");
        var matchedOrg2 = await _repository.GetOrganizationAsync("920212345");
        Assert.NotNull(matchedOrg1);
        Assert.Equal(3, matchedOrg1.NotificationAddresses.Count);
        Assert.Null(matchedOrg2);

        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1");

        // Act
        var numberOfUpdatedAddresses = await _repository.SyncNotificationAddressesAsync(changes);
        var updatedOrg1 = await _repository.GetOrganizationAsync("123456789");
        var updatedOrg2 = await _repository.GetOrganizationAsync("920212345");

        // Assert
        Assert.NotNull(updatedOrg1);
        Assert.Equal(4, updatedOrg1.NotificationAddresses.Count);
        Assert.NotNull(updatedOrg2);
        Assert.Single(updatedOrg2.NotificationAddresses);
        Assert.Equal(2, numberOfUpdatedAddresses);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithDeleteData_ReturnsUpdatedRows()
    {
        var matchedOrg = await _repository.GetOrganizationAsync("987654321");
        Assert.Equal(2, matchedOrg.NotificationAddresses.Count);

        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_4");

        // Act
        var numberOfUpdatedAddresses = await _repository.SyncNotificationAddressesAsync(changes);
        var updatedOrg = await _repository.GetOrganizationAsync("987654321");

        // Assert
        Assert.Single(updatedOrg.NotificationAddresses);
        Assert.Equal(1, numberOfUpdatedAddresses);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WithDeleteData_ReturnsNullWhenAlreadyDeleted()
    {
        var matchedOrg = await _repository.GetOrganizationAsync("987654321");
        Assert.Equal(2, matchedOrg.NotificationAddresses.Count);

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
    public async Task SyncNotificationAddressesAsync_WithUpdateData_ReturnsUpdatedRows()
    {
        var matchedOrg = await _repository.GetOrganizationAsync("987654321");
        Assert.Equal(2, matchedOrg.NotificationAddresses.Count);

        var changes = await TestDataLoader.Load<NotificationAddressChangesLog>("changes_5");

        // Act
        var numberOfUpdatedAddresses = await _repository.SyncNotificationAddressesAsync(changes);
        var updatedOrg = await _repository.GetOrganizationAsync("987654321");

        // Assert
        Assert.Equal(2, updatedOrg.NotificationAddresses.Count);
        Assert.Equal(matchedOrg.NotificationAddresses[1].RegistryID, updatedOrg.NotificationAddresses[1].RegistryID);
        Assert.NotEqual(matchedOrg.NotificationAddresses[1].Address, updatedOrg.NotificationAddresses[1].Address);
        Assert.True(numberOfUpdatedAddresses > 0);
    }

    [Fact]
    public async Task GetOrganizations_WhenFound_ReturnsWithNotificationAddresses()
    {
        // Arrange
        var orgNumberLookup = new List<string>() { "123456789", "987654321" };

        var expectedOrg1 = _organizationTestData
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

    private static void AssertRegisterProperties(OrganizationDE expected, OrganizationDE actual)
    {
        Assert.Equal(expected.RegistryOrganizationNumber, actual.RegistryOrganizationNumber);
        Assert.Equal(expected.RegistryOrganizationId, actual.RegistryOrganizationId);
    }
}
