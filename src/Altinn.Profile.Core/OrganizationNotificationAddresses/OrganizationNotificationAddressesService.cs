using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Implementation of the <see cref="IOrganizationNotificationAddressesService"/>, using an <see cref="IOrganizationNotificationAddressRepository"/> to interact with notification addresses of organizations "/>
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="OrganizationNotificationAddressesService"/> class.
    /// </remarks>
    public class OrganizationNotificationAddressesService(IOrganizationNotificationAddressRepository orgRepository, IOrganizationNotificationAddressUpdateClient updateClient) : IOrganizationNotificationAddressesService
    {
        private readonly IOrganizationNotificationAddressRepository _orgRepository = orgRepository;
        private readonly IOrganizationNotificationAddressUpdateClient _updateClient = updateClient;

        /// <summary>
        /// Method for creating a notification addresses for an organization. Data is written primarily to an <see cref="IOrganizationNotificationAddressUpdateClient"/> and lastly to the <see cref="IOrganizationNotificationAddressRepository"/>.
        /// </summary>
        /// <param name="organizationNumber">An organization number to indicate which organization to update addresses for</param>
        /// <param name="notificationAddress">The new notification address</param>
        /// <param name="cancellationToken">To cancel the request before it is finished</param>
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

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken)
        {
            var result = await _orgRepository.GetOrganizationsAsync(organizationNumbers, cancellationToken);

            return result;
        }
    }
}
