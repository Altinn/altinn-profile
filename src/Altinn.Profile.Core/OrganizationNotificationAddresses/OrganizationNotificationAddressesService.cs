using Altinn.Platform.Register.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;

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
        public async Task<Organization> CreateNotificationAddress(string organizationNumber, NotificationAddress notificationAddress, CancellationToken cancellationToken)
        {
            var orgs = await _orgRepository.GetOrganizationsAsync([organizationNumber], cancellationToken);
            var org = orgs.FirstOrDefault();
            org ??= new Organization { OrganizationNumber = organizationNumber, NotificationAddresses = [] };

            var existingAddress = org.NotificationAddresses?.FirstOrDefault(x => x.FullAddress == notificationAddress.FullAddress && x.AddressType == notificationAddress.AddressType);
            if (existingAddress != null)
            {
                existingAddress.UpdateMessage = "Notification address already exists";
                return org;
            }

            try
            {
                var (registryId, errorMessage) = await _updateClient.CreateNewNotificationAddress(notificationAddress, organizationNumber);
                if (registryId == null)
                {
                    notificationAddress.UpdateMessage = errorMessage;
                    org.NotificationAddresses!.Add(notificationAddress);
                    return org;
                }

                notificationAddress.RegistryID = registryId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create notification address {notificationAddress.FullAddress} for organization {organizationNumber}", ex);
            }

            return await _orgRepository.CreateNotificationAddressAsync(organizationNumber, notificationAddress);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken)
        {
            var result = await _orgRepository.GetOrganizationsAsync(organizationNumbers, cancellationToken);

            return result;
        }
    }
}
