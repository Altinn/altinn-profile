﻿using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc />
public class OrganizationNotificationAddressRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IOrganizationNotificationAddressUpdater
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
    private readonly string _organizationNumberConst = "ORGANISASJONSNUMMER";

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
                updates += await UpdateNotificationAddressAsync(address);
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
    /// A task that represents the asynchronous operation.
    /// </returns>
    private async Task<int> UpdateNotificationAddressAsync(Entry address)
    {
        var orgNumber = address?.Content?.ContactPoint?.UnitContactInfo?.UnitIdentifier?.Value;
        if (orgNumber == null || address?.Content?.ContactPoint?.UnitContactInfo?.UnitIdentifier?.Type != _organizationNumberConst)
        {
            return 0;
        }

        var organization = await GetOrganization(orgNumber);
        organization ??= await CreateOrganization(orgNumber);

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
    public async Task<Organization?> GetOrganization(string orgNumber)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        return await databaseContext.Organizations
                .Include(o => o.NotificationAddresses)
                .FirstOrDefaultAsync(o => o.RegistryOrganizationNumber == orgNumber);
    }
    
    private async Task<Organization> CreateOrganization(string orgNumber)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var organization = new Organization
        {
            RegistryOrganizationNumber = orgNumber,
            NotificationAddresses = [],
        };
        
        await databaseContext.Organizations.AddAsync(organization);
        await databaseContext.SaveChangesAsync();

        return organization;
    }
}
