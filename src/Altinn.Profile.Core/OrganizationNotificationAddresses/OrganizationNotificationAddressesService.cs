using Altinn.Platform.Register.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Implementation of the <see cref="IOrganizationNotificationAddressesService"/> interface using an <see cref="IOrganizationNotificationAddressRepository"/> retrieve profile data "/>
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="OrganizationNotificationAddressesService"/> class.
    /// </remarks>
    public class OrganizationNotificationAddressesService(IOrganizationNotificationAddressRepository orgRepository, IOrganizationNotificationAddressUpdateClient updateClient) : IOrganizationNotificationAddressesService
    {
        private readonly IOrganizationNotificationAddressRepository _orgRepository = orgRepository;
        private readonly IOrganizationNotificationAddressUpdateClient _updateClient = updateClient;

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken)
        {
            var result = await _orgRepository.GetOrganizationsAsync(organizationNumbers, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<Organization> UpdateOrganizationNotificationAddresses(string organizationNumber, IEnumerable<NotificationAddress> notificationAddresses, CancellationToken cancellationToken)
        {
            var organization = (await _orgRepository.GetOrganizationsAsync([organizationNumber], cancellationToken)).First();
            var validAddreses = CleanInput(notificationAddresses, organization).ToList();

            foreach (var notificationAddress in validAddreses)
            {
                if (notificationAddress.IsNew)
                {
                    await CreateNotificationAddress(organization, notificationAddress);
                }
                else if (notificationAddress.ToBeDeleted == true)
                {
                    await DeleteNotificationAddress(organization, notificationAddress);
                }
                else
                {
                    await UpdateNotificationAddress(organization, notificationAddress);
                }
            }

            var updatedOrganization = (await _orgRepository.GetOrganizationsAsync([organizationNumber], cancellationToken)).First();

            return updatedOrganization;
        }

        private async Task CreateNotificationAddress(Organization organization, NotificationAddress notificationAddress)
        {
            try
            {
                await _updateClient.CreateNewNotificationAddress(notificationAddress, organization);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create notification address {notificationAddress.FullAddress} for organization {organization.OrganizationNumber}", ex);
            }

            await _orgRepository.CreateNotificationAddressAsync(organization, notificationAddress);
        }

        private async Task DeleteNotificationAddress(Organization organization, NotificationAddress notificationAddress)
        {
            try
            {
                await _updateClient.DeleteNotificationAddress(notificationAddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete notification address {notificationAddress.FullAddress} for organization {organization.OrganizationNumber}", ex);
            }

            notificationAddress.IsSoftDeleted = true;
            await _orgRepository.UpdateNotificationAddressAsync(organization, notificationAddress);
        }

        private async Task UpdateNotificationAddress(Organization organization, NotificationAddress notificationAddress)
        {
            var oldAddress = organization.NotificationAddresses?.FirstOrDefault(x => x.NotificationAddressID == notificationAddress.NotificationAddressID);

            // Checks if this instance has changed address data compared to the old one
            if (oldAddress.Address == notificationAddress.Address && oldAddress.Domain == notificationAddress.Domain)
            {
                return;
            }

            try
            {
                await _updateClient.DeleteNotificationAddress(notificationAddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update notification address {notificationAddress.FullAddress} for organization {organization.OrganizationNumber}", ex);
            }

            await _orgRepository.UpdateNotificationAddressAsync(organization, notificationAddress);
        }

        private IEnumerable<NotificationAddress> CleanInput(IEnumerable<NotificationAddress> notificationAddresses, Organization organization)
        {
            var notNullAddresses = notificationAddresses
                .Where(x => !string.IsNullOrWhiteSpace(x.Address) && !(x.ToBeDeleted == true && x.IsNew));

            var validAddreses = notNullAddresses.Where(x => x.ToBeDeleted != true).ToList();
            foreach (var address in notNullAddresses.Where(x => x.ToBeDeleted == true))
            {
                if (address.ToBeDeleted == true)
                {
                    var existingAddress = organization.NotificationAddresses?.FirstOrDefault(n => n.NotificationAddressID == address.NotificationAddressID);

                    // You are not allowed to remove all endpoints
                    if (existingAddress == null || validAddreses.Where(n => n.IsSoftDeleted == false && n.ToBeDeleted == false).Count() <= 1)
                    {
                        continue;
                    }

                    address.IsSoftDeleted = existingAddress.IsSoftDeleted;

                    validAddreses.Add(address);
                }
            }

            // remove duplicates
            var duplicateGroups = validAddreses
                .Where(x => x.ToBeDeleted == false && x.IsSoftDeleted == false)
                .GroupBy(x => new { x.FullAddress, x.AddressType })
                .Where(x => x.Count() > 1);
            foreach (var duplicateGroup in duplicateGroups)
            {
                foreach (NotificationAddress endpoint in duplicateGroup.OrderByDescending(x => x.NotificationAddressID).Skip(1))
                {
                    validAddreses.Remove(endpoint);
                }
            }

            return validAddreses;
        }
    }
}
