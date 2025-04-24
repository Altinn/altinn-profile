using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Implementation of the <see cref="IOrganizationNotificationAddressesService"/> interface using an <see cref="IOrganizationNotificationAddressRepository"/> to interact with notification addresses of organizations "/>
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

            var (registryId, errorMessage) = await _updateClient.CreateNewNotificationAddress(notificationAddress, organizationNumber);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                notificationAddress.UpdateMessage = errorMessage;
                org.NotificationAddresses!.Add(notificationAddress);
                return org;
            }

            notificationAddress.RegistryID = registryId;

            var updatedOrg = await _orgRepository.CreateNotificationAddressAsync(organizationNumber, notificationAddress);

            return updatedOrg;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken)
        {
            var result = await _orgRepository.GetOrganizationsAsync(organizationNumbers, cancellationToken);

            return result;
        }
    }
}
