using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Altinn.Profile.Models.OrgNotificationAddressesResponse;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organization notifications address API endpoints for internal consumption (e.g. Notifications) requiring neither authenticated user token nor access token authorization.
    /// </summary>
    [Route("profile/api/v1/organizations/notificationaddresses")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class OrgNotificationAddressController : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrgNotificationAddressController"/> class.
        /// </summary>
        public OrgNotificationAddressController(IOrganizationNotificationAddressesService notificationAddressService)
        {
            _notificationAddressService = notificationAddressService;
        }

        /// <summary>
        /// Endpoint looking up the notification addresses for the organization provided in the lookup object in the request body
        /// </summary>
        /// <returns>Returns an overview of the user registered notification addresses for the provided organization</returns>
        [HttpPost("lookup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrgNotificationAddressesResponse>> PostLookup([FromBody] OrgNotificationAddressRequest orgContactPointLookup, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddresses(orgContactPointLookup.OrganizationNumbers, cancellationToken, true);

            OrgNotificationAddressesResponse result = MapResult(organizations);
            return Ok(result);
        }

        private static OrgNotificationAddressesResponse MapResult(IEnumerable<Organization> organizations)
        {
            var orgContacts = new OrgNotificationAddressesResponse();
            foreach (var organization in organizations)
            {
                var contactPoints = new NotificationAddresses
                {
                    OrganizationNumber = organization.OrganizationNumber,
                };

                if (organization.NotificationAddresses?.Count > 0)
                {
                    foreach (var notificationAddress in organization.NotificationAddresses)
                    {
                        if (notificationAddress.IsSoftDeleted == true || notificationAddress.HasRegistryAccepted == false)
                        {
                            continue;
                        }

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
