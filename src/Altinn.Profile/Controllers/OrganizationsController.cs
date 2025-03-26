using System.Collections.Generic;
using System.Linq;
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
    /// Controller for organization notifications address API endpoints for external usage
    /// </summary>
    [Route("profile/api/v1/organizations/{organizationNumber}/notificationaddresses")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsController"/> class.
        /// </summary>
        public OrganizationsController(IOrganizationNotificationAddressesService notificationAddressService)
        {
            _notificationAddressService = notificationAddressService;
        }

        /// <summary>
        /// Endpoint looking up the notification addresses for the organization provided in the lookup object in the request body
        /// </summary>
        /// <returns>Returns an overview of the user registered notification addresses for the provided organization</returns>
        [HttpGet("mandatory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrganizationResponse>> GetMandatory([FromRoute]string organizationNumber, CancellationToken cancellationToken)
        {
            // Check user has access to org number
            var organizations = await _notificationAddressService.GetOrganizationNotificationAddresses([organizationNumber], cancellationToken);
            if (organizations.Count() == 0)
            {
                return NotFound();
            }

            if (organizations.Count() > 1)
            {
                // We have a problem
            }

            var response = MapResponse(organizations);

            return Ok(response);
        }

        private OrganizationResponse MapResponse(IEnumerable<Organization> organizations)
        {
            var organization = organizations.First();

            var result = new OrganizationResponse 
            { 
                OrganizationNumber = organization.OrganizationNumber,
                NotificationAddresses = organization.NotificationAddresses.Where(n => n.IsSoftDeleted != true).Select(MapNotificationAddress).ToList()
            };

            return result;
        }

        private OrganizationResponse.NotificationAddress MapNotificationAddress(NotificationAddress notificationAddress)
        {
            var response = new OrganizationResponse.NotificationAddress
            {
                RegistryID = notificationAddress.RegistryID,
                NotificationAddressID = notificationAddress.NotificationAddressID,
            };
            if (notificationAddress.AddressType == AddressType.Email)
            {
                response.Email = notificationAddress.FullAddress;
            }
            else
            {
                response.Phone = notificationAddress.Address;
                response.CountryCode = notificationAddress.Domain;
            }

            return response;
        }
    }
}
