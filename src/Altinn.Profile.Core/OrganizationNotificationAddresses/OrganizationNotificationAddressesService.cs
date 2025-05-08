using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationNotificationAddressesService"/> class to interact with notification addresses of organizations.
    /// </summary>
    /// <param name="orgRepository">The repository for organization notification addresses</param>
    /// <param name="updateClient">The client for updating organization notification addresses</param>
    public class OrganizationNotificationAddressesService(IOrganizationNotificationAddressRepository orgRepository, IOrganizationNotificationAddressUpdateClient updateClient) : IOrganizationNotificationAddressesService
    {
        private readonly IOrganizationNotificationAddressRepository _orgRepository = orgRepository;
        private readonly IOrganizationNotificationAddressUpdateClient _updateClient = updateClient;

        /// <inheritdoc/>
        public async Task<NotificationAddress> CreateNotificationAddress(string organizationNumber, NotificationAddress notificationAddress, CancellationToken cancellationToken)
        {
            var orgs = await _orgRepository.GetOrganizationsAsync([organizationNumber], cancellationToken);
            var org = orgs.FirstOrDefault();
            org ??= new Organization { OrganizationNumber = organizationNumber, NotificationAddresses = [] };

            var existingAddress = org.NotificationAddresses?.FirstOrDefault(x => x.FullAddress == notificationAddress.FullAddress && x.AddressType == notificationAddress.AddressType);
            if (existingAddress != null)
            {
                return existingAddress;
            }

            var registryId = await _updateClient.CreateNewNotificationAddress(notificationAddress, organizationNumber);

            var updatedNotificationAddress = await _orgRepository.CreateNotificationAddressAsync(organizationNumber, notificationAddress, registryId);

            return updatedNotificationAddress;
        }

        /// <summary>
        /// Method for updating a notification address for an organization. Data is written primarily to an <see cref="IOrganizationNotificationAddressUpdateClient"/> and lastly to the <see cref="IOrganizationNotificationAddressRepository"/>.
        /// </summary>
        /// <param name="organizationNumber">The organization number of the organization the notification address belongs to.</param>
        /// <param name="notificationAddress">The notification address with updated data</param>
        /// <param name="cancellationToken">To cancel the request before it is finished</param>
        public async Task<NotificationAddress?> UpdateNotificationAddress(string organizationNumber, NotificationAddress notificationAddress, CancellationToken cancellationToken)
        {
            var orgs = await _orgRepository.GetOrganizationsAsync([organizationNumber], cancellationToken);
            var org = orgs.FirstOrDefault();

            if (org == null)
            {
                return null;
            }

            var existingNotificationAddress = org.NotificationAddresses?.FirstOrDefault(n => n.NotificationAddressID == notificationAddress.NotificationAddressID);
            if (existingNotificationAddress == null)
            {
                return null;
            }

            var registryId = await _updateClient.UpdateNotificationAddress(existingNotificationAddress.RegistryID, notificationAddress, organizationNumber);

            var updatedNotificationAddress = await _orgRepository.UpdateNotificationAddressAsync(notificationAddress, registryId);

            return updatedNotificationAddress;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken)
        {
            var result = await _orgRepository.GetOrganizationsAsync(organizationNumbers, cancellationToken);

            return result;
        }
    }
}
