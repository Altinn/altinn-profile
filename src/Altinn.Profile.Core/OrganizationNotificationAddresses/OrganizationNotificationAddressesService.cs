using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using static Altinn.Profile.Core.OrganizationNotificationAddresses.OrgContactPointsResponse;

namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Implementation of the <see cref="IOrganizationNotificationAddressesService"/> interface using an <see cref="IUnitProfileRepository"/> retrieve profile data "/>
    /// </summary>
    public class OrganizationNotificationAddressesService : IOrganizationNotificationAddressesService
    {
        private readonly IOrganizationNotificationAddressRepository _orgRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitContactPointService"/> class.
        /// </summary>
        public OrganizationNotificationAddressesService(IOrganizationNotificationAddressRepository orgRepository)
        {
            _orgRepository = orgRepository;
        }

        /// <inheritdoc/>
        public async Task<OrgContactPointsResponse> GetNotificationContactPoints(OrgContactPointLookupRequest lookup, CancellationToken cancellationToken)
        {
            var result = await _orgRepository.GetOrganizationsAsync(lookup, cancellationToken);

            return MapResult(result);
        }

        private static OrgContactPointsResponse MapResult(IEnumerable<Organization> organizations)
        {
            var orgContacts = new OrgContactPointsResponse();
            foreach (var organization in organizations)
            {
                var contactPoints = new OrganizationContactPoints
                {
                    OrganizationNumber = organization.OrganizationNumber,
                };

                if (organization.NotificationAddresses?.Count > 0)
                {
                    foreach (var notificationAddress in organization.NotificationAddresses)
                    {
                        switch (notificationAddress.AddressType)
                        {
                            case AddressType.Email:
                                contactPoints.EmailList.Add(notificationAddress.FullAddress);
                                break;
                            case AddressType.SMS:
                                contactPoints.MobileNumberList.Add(notificationAddress.FullAddress);
                                break;
                        }
                    }
                }

                orgContacts.ContactPointsList.Add(contactPoints);
            }

            return orgContacts;
        }
    }
}
