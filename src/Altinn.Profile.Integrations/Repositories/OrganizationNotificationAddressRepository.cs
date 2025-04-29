using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Altinn.Profile.Integrations.Persistence;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc />
public class OrganizationNotificationAddressRepository(IDbContextFactory<ProfileDbContext> contextFactory, IMapper mapper) : IOrganizationNotificationAddressUpdater, IOrganizationNotificationAddressRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
    private readonly IMapper _mapper = mapper;

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

        var organization = await GetOrganizationAsync(orgNumber);
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

        var organizationNotificationAddress = DataMapper.MapOrganizationNotificationAddress(address, organization);

        var existingAddress = organization.NotificationAddresses?.FirstOrDefault(a => a.RegistryID == address.Id);
        if (existingAddress == null)
        {
            databaseContext.NotificationAddresses.Add(organizationNotificationAddress);
        }
        else
        {
            existingAddress.Address = organizationNotificationAddress.Address;
            existingAddress.FullAddress = organizationNotificationAddress.FullAddress;
            existingAddress.Domain = organizationNotificationAddress.Domain;
            existingAddress.RegistryUpdatedDateTime = organizationNotificationAddress.RegistryUpdatedDateTime;
            existingAddress.UpdateSource = organizationNotificationAddress.UpdateSource;
            existingAddress.IsSoftDeleted = organizationNotificationAddress.IsSoftDeleted;

            databaseContext.NotificationAddresses.Update(existingAddress);
        }

        return await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets an organization with connected notification addresses form the database
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task<OrganizationDE?> GetOrganizationAsync(string orgNumber)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        return await databaseContext.Organizations
                .Include(o => o.NotificationAddresses)
                .FirstOrDefaultAsync(o => o.RegistryOrganizationNumber == orgNumber);
    }
    
    private async Task<int> CreateOrganizationWithNotificationAddress(string orgNumber, Entry address)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var organization = new OrganizationDE
        {
            RegistryOrganizationNumber = orgNumber,
            NotificationAddresses = [],
        };
        var organizationNotificationAddress = DataMapper.MapOrganizationNotificationAddress(address, organization);
        organization.NotificationAddresses.Add(organizationNotificationAddress);

        await databaseContext.Organizations.AddAsync(organization);
        
        return await databaseContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Organization>> GetOrganizationsAsync(List<string> organizationNumbers, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var foundOrganizations = await databaseContext.Organizations
                .Include(o => o.NotificationAddresses)
                .Where(o => organizationNumbers.Contains(o.RegistryOrganizationNumber))
                .ToListAsync(cancellationToken);

        if (foundOrganizations.Count == 0)
        {
            return [];
        }

        return foundOrganizations.Select(_mapper.Map<Organization>);
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

        var organizationNotificationAddress = DataMapper.MapFromCoreModelNotificationAddress(orgDE, notificationAddress, registryId);

        orgDE.NotificationAddresses!.Add(organizationNotificationAddress);
        databaseContext.Organizations.Update(orgDE);

        await databaseContext.SaveChangesAsync();

        return _mapper.Map<NotificationAddress>(organizationNotificationAddress);
    }
}
