using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Altinn.Profile.Models.OrgContactPointsResponse;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organization contact point API endpoints for internal consumption (e.g. Notifications) requiring neither authenticated user token nor access token authorization.
    /// </summary>
    [Route("profile/api/v1/organizations/contactpoint")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class OrgContactPointController : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrgContactPointController"/> class.
        /// </summary>
        public OrgContactPointController(IOrganizationNotificationAddressesService notificationAddressService)
        {
            _notificationAddressService = notificationAddressService;
        }

        /// <summary>
        /// Endpoint looking up the contact points for the organization provided in the lookup object in the request body
        /// </summary>
        /// <returns>Returns an overview of the user registered contact points for the provided organization</returns>
        [HttpPost("lookup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrgContactPointsResponse>> PostLookup([FromBody] OrgContactPointLookupRequest orgContactPointLookup, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var organizations = await _notificationAddressService.GetNotificationContactPoints(orgContactPointLookup.OrganizationNumbers, cancellationToken);

            OrgContactPointsResponse result = MapResult(organizations);
            return Ok(result);
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
