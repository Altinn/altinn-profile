using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Mappers;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = "PlatformAccess")]
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
            var organizations = await _notificationAddressService.GetOrganizationNotificationAddresses([organizationNumber], cancellationToken);
            if (!organizations.Any())
            {
                return NotFound();
            }

            var organization = organizations.First();

            var response = OrganizationResponseMapper.MapResponse(organization);

            return Ok(response);
        }
    }
}
