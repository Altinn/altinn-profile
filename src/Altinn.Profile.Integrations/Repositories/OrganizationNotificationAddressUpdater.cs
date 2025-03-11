using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc />
public class OrganizationNotificationAddressUpdater(IDbContextFactory<ProfileDbContext> contextFactory) : IOrganizationNotificationAddressUpdater
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
    private readonly string organizationNumberConst = "ORGANISASJONSNUMMER";

    /// <inheritdoc />
    public async Task<int> SyncNotificationAddressesAsync(NotificationAddressChangesLog organizationNotificationAddressChanges)
    {
        var addresses = organizationNotificationAddressChanges.OrganizationNotificationAddressList!;
        foreach (var address in addresses) 
        {
            if (address.IsDeleted == true)
            {
                await DeleteNotificationAddressAsync(address);
            }
            else
            {
                await UpdateNotificationAddressAsync(address);
            }
        }
    }

    public async Task<int> DeleteNotificationAddressAsync(Entry address)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var entry = await databaseContext.NotificationAddresses.FirstOrDefaultAsync(a => a.RegistryID == address.Id);
        if (entry == null)
        {
            return 0;
        }

        databaseContext.Remove(entry);

        return await databaseContext.SaveChangesAsync();
    }

    public async Task<int> UpdateNotificationAddressAsync(Entry address)
    {
        var organization = await GetOrganization(address.Content.ContactPoint.UnitContactInfo.UnitIdentifier.Type, address.Content.ContactPoint.UnitContactInfo.UnitIdentifier.Value);
        if (organization == null)
        {
            // Add new organization
        }
        else
        {
            var existingAddress = organization.NotificationAddresses?.FirstOrDefault(a => a.RegistryID == address.Id);
            if (existingAddress == null)
            {
                // Add address
            }
            else
            {
            }
        }
        
        var organizationNotificationAddress = new OrganizationNotificationAddress
        {
            RegistryID = address.Id,
            RegistryOrganizationID = string.Empty,
            RegistryOrganizationNumber = address.Content.ContactPoint.UnitContactInfo.UnitIdentifier.Value,
            AddressType = adress.

        }
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var entry = await databaseContext.NotificationAddresses.FirstOrDefaultAsync(a => a.RegistryID == address.Id);
        if (entry == null)
        {
            databaseContext.NotificationAddresses.Add(organizationNotificationAddress);
        }

        
        
        

        return await databaseContext.SaveChangesAsync();
    }

    private async Task<Organization?> GetOrganization(string identificatortype, string orgNumber)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        if (identificatortype == organizationNumberConst)
        {
            return await databaseContext.Organizations.FirstOrDefaultAsync(o => o.RegistryOrganizationNumber == orgNumber);
        }

        return null;
    }

    private OrganizationNotificationAddress MapOrganizationNotificationAddress(Entry entry)
    {
        var organizationNotificationAddress = new OrganizationNotificationAddress
        {
            RegistryID = entry.Id,
            RegistryOrganizationID = 0, // TODO
            RegistryUpdatedDateTime = entry.Updated,
            UpdateSource = UpdateSource.KoFuVi,
            HasRegistryAccepted = true,
            IsSoftDeleted = false,
            NotificationName = entry.Title,
        };

        var contanctPoint = entry.Content.ContactPoint.DigitalContactPoint;
        var isEmail = contanctPoint.EmailAddress != null;

        if (isEmail)
        {
            organizationNotificationAddress.AddressType = AddressType.Email;
            organizationNotificationAddress.Domain = contanctPoint.EmailAddress.Domain;
            organizationNotificationAddress.Address = contanctPoint.EmailAddress.Username;
            organizationNotificationAddress.FullAddress = contanctPoint.EmailAddress.Name;

        }
        else
        {
            organizationNotificationAddress.AddressType = AddressType.SMS;
            organizationNotificationAddress.Domain = contanctPoint.PhoneNumber.Prefix;
            organizationNotificationAddress.Address = contanctPoint.PhoneNumber.NationalNumber;
            organizationNotificationAddress.FullAddress = contanctPoint.PhoneNumber.Number;
        }

        return organizationNotificationAddress;
    }
}
