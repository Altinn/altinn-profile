using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Mappings;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc />
public class OrganizationNotificationAddressRepository(IDbContextFactory<ProfileDbContext> contextFactory, Telemetry? telemetry) : IOrganizationNotificationAddressUpdater, IOrganizationNotificationAddressRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
    private readonly Telemetry? _telemetry = telemetry;

    /// <inheritdoc />
    public async Task<int> SyncNotificationAddressesAsync(NotificationAddressChangesLog organizationNotificationAddressChanges)
    {
        var addresses = organizationNotificationAddressChanges.OrganizationNotificationAddressList!;
        var updates = 0;
        foreach (var address in addresses) 
        {
            if (address.IsDeleted == true)
            {
                updates += await DeleteNotificationAddressAsync(address.Id);
            }
            else
            {
                updates += await UpsertOrganizationWithNotificationAddressAsync(address);
            }
        }

        return updates;
    }

    /// <summary>
    /// Deletes notification addresses in the DB for organizations
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    private async Task<int> DeleteNotificationAddressAsync(string? addressId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var entry = await databaseContext.NotificationAddresses.FirstOrDefaultAsync(a => a.RegistryID == addressId);
        if (entry == null)
        {
            return 0;
        }

        databaseContext.Remove(entry);
        _telemetry?.AddressDeleted();

        return await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Updates or creates notification addresses in the DB for organizations
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation with the number of written rows. 
    /// </returns>
    private async Task<int> UpsertOrganizationWithNotificationAddressAsync(Entry address)
    {
        var orgNumber = address.Content?.ContactPoint?.UnitContactInfo?.UnitIdentifier?.Value;
        if (orgNumber == null || address.Content?.ContactPoint?.UnitContactInfo?.UnitIdentifier?.Type != DataMapper.OrganizationNumberType)
        {
            return 0;
        }

        var organization = await GetOrganizationDEAsync(orgNumber, CancellationToken.None);
        if (organization is null)
        {
            return await CreateOrganizationWithNotificationAddress(orgNumber, address);
        }

        return await UpdateNotificationAddressAsync(address, organization);
    }

    /// <summary>
    /// Updates or creates a notification address in the DB for the given organization
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.  The result contains a bit (1 or 0) indicating whether the address was updated.
    /// </returns>
    private async Task<int> UpdateNotificationAddressAsync(Entry address, OrganizationDE organization)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var existingAddress = organization.NotificationAddresses?.FirstOrDefault(a => a.RegistryID == address.Id);

        if (existingAddress == null)
        {
            var organizationNotificationAddress = DataMapper.PopulateOrganizationNotificationAddress(organization, address);
            databaseContext.NotificationAddresses.Add(organizationNotificationAddress);
            _telemetry?.AddressAdded();
        }
        else
        {
            var updatedAddress = DataMapper.PopulateExistingOrganizationNotificationAddress(existingAddress, address);
            databaseContext.NotificationAddresses.Update(updatedAddress);
            _telemetry?.AddressUpdated();
        }

        return await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets organizations with notification addresses matching the specified email address from the database
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task<OrganizationDE?> GetOrganizationDEAsync(string orgNumber, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await databaseContext.Organizations
                .Include(o => o.NotificationAddresses)
                .FirstOrDefaultAsync(o => o.RegistryOrganizationNumber == orgNumber, cancellationToken);
    }
    
    private async Task<int> CreateOrganizationWithNotificationAddress(string orgNumber, Entry address)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var organization = new OrganizationDE
        {
            RegistryOrganizationNumber = orgNumber,
            NotificationAddresses = [],
        };
        var organizationNotificationAddress = DataMapper.PopulateOrganizationNotificationAddress(organization, address);
        organization.NotificationAddresses.Add(organizationNotificationAddress);

        await databaseContext.Organizations.AddAsync(organization);
        _telemetry?.OrganizationAdded();
        _telemetry?.AddressAdded();

        return await databaseContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<Organization?> GetOrganizationAsync(string organizationNumber, CancellationToken cancellationToken)
    {
        var organization = await GetOrganizationDEAsync(organizationNumber, cancellationToken);

        return OrganizationMapper.MapFromDataEntity(organization);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Organization>> GetOrganizationsAsync(List<string> organizationNumbers, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var foundOrganizations = await databaseContext.Organizations
                .Include(o => o.NotificationAddresses.Where(a => a.IsSoftDeleted != true))
                .Where(o => organizationNumbers.Contains(o.RegistryOrganizationNumber))
                .ToListAsync(cancellationToken);

        if (foundOrganizations.Count == 0)
        {
            return [];
        }

        return foundOrganizations.Select(OrganizationMapper.MapFromDataEntity).Where(org => org != null)!;
    }

    /// <summary>
    /// Gets list of notification addresses for an organization by email address from the database
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task<IEnumerable<OrganizationDE>> GetOrganizationNotificationAddressesByEmailAddressDEAsync(string emailAddress, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = await databaseContext.Organizations
            .Where(o => o.NotificationAddresses.Any(na =>
                na.AddressType == AddressType.Email &&
                na.FullAddress != null &&
                EF.Functions.ILike(na.FullAddress, emailAddress) &&
                na.IsSoftDeleted != true))
            .Include(o => o.NotificationAddresses.Where(na =>
                na.AddressType == AddressType.Email &&
                na.FullAddress != null &&
                na.IsSoftDeleted != true))
            .ToListAsync(cancellationToken);

        return query;
    }

    /// <summary>
    /// Gets organizations with notification addresses matching the specified email address from the database
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task<IEnumerable<Organization>> GetOrganizationNotificationAddressesByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken)
    {
        var foundOrganizations = await GetOrganizationNotificationAddressesByEmailAddressDEAsync(emailAddress, cancellationToken);

        return foundOrganizations.Select(OrganizationMapper.MapFromDataEntity).Where(org => org != null)!;
    }

    /// <inheritdoc/>
    public async Task<NotificationAddress> CreateNotificationAddressAsync(string organizationNumber, NotificationAddress notificationAddress, string registryId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var orgDE = await databaseContext.Organizations
            .Include(o => o.NotificationAddresses)
            .FirstOrDefaultAsync(o => o.RegistryOrganizationNumber == organizationNumber);

        orgDE ??= new OrganizationDE
            {
                RegistryOrganizationNumber = organizationNumber,
                NotificationAddresses = [],
            };

        var organizationNotificationAddress = DataMapper.MapFromCoreModelForNewNotificationAddress(orgDE, notificationAddress, registryId);

        orgDE.NotificationAddresses!.Add(organizationNotificationAddress);
        databaseContext.Organizations.Update(orgDE);

        await databaseContext.SaveChangesAsync();

        return OrganizationMapper.MapFromDataEntity(organizationNotificationAddress);
    }

    /// <inheritdoc/>
    public async Task<NotificationAddress> UpdateNotificationAddressAsync(NotificationAddress notificationAddress, string registryId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var notificationAddressDE = await databaseContext.NotificationAddresses.FirstAsync(n => n.NotificationAddressID == notificationAddress.NotificationAddressID);

        notificationAddressDE.AddressType = notificationAddress.AddressType;
        notificationAddressDE.Address = notificationAddress.Address;
        notificationAddressDE.Domain = notificationAddress.Domain;
        notificationAddressDE.FullAddress = notificationAddress.FullAddress;
        notificationAddressDE.RegistryID = registryId;

        databaseContext.NotificationAddresses.Update(notificationAddressDE);

        await databaseContext.SaveChangesAsync();

        return OrganizationMapper.MapFromDataEntity(notificationAddressDE);
    }
    
    /// <inheritdoc/>
    public async Task<NotificationAddress> DeleteNotificationAddressAsync(int notificationAddressId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var notificationAddressDE = await databaseContext.NotificationAddresses.FirstAsync(n => n.NotificationAddressID == notificationAddressId);

        notificationAddressDE.IsSoftDeleted = true;

        databaseContext.NotificationAddresses.Update(notificationAddressDE);

        await databaseContext.SaveChangesAsync();

        return OrganizationMapper.MapFromDataEntity(notificationAddressDE);
    }

    /// <inheritdoc/>
    public async Task<NotificationAddress> RestoreNotificationAddress(int notificationAddressId, string registryId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var notificationAddressDE = await databaseContext.NotificationAddresses.FirstAsync(n => n.NotificationAddressID == notificationAddressId);

        notificationAddressDE.IsSoftDeleted = false;
        notificationAddressDE.RegistryID = registryId;

        databaseContext.NotificationAddresses.Update(notificationAddressDE);

        await databaseContext.SaveChangesAsync();

        return OrganizationMapper.MapFromDataEntity(notificationAddressDE);
    }
}
