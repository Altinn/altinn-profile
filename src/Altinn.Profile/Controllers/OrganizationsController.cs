using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Authorization;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Mappers;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        /// Endpoint looking up the notification addresses for the given organization
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the provided organization</returns>
        [HttpGet("mandatory")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrganizationResponse>> GetMandatory([FromRoute]string organizationNumber, CancellationToken cancellationToken)
        {
            var organizations = await _notificationAddressService.GetOrganizationNotificationAddresses([organizationNumber], cancellationToken);

            var orgCount = organizations.Count();

            if (orgCount == 0)
            {
                return NotFound();
            }
            else if (orgCount > 1)
            {
                throw new InvalidOperationException("Indecisive organization result");
            }

            var organization = organizations.First();

            var response = OrganizationResponseMapper.MapResponse(organization);

            return Ok(response);
        }

        /// <summary>
        /// Endpoint looking up the notification addresses for the given organization
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the provided organization</returns>
        [HttpPost("mandatory")]
        ////[Authorize(Policy = AuthConstants.OrgNotificationAddress_Write)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OrganizationResponse>> CreateNotificationAddress([FromRoute] string organizationNumber, [FromBody] NotificationAddressModel request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(organizationNumber))
            {
                return BadRequest("Organization number is required");
            }

            var notificationAddresses = OrganizationRequestMapper.MapNotificationAddress(request);

            var organization = await _notificationAddressService.CreateNotificationAddress(organizationNumber, notificationAddresses, cancellationToken);

            var response = OrganizationResponseMapper.MapResponse(organization);

            return Created($"profile/api/v1/organizations/{organizationNumber}/notificationaddresses/mandatory", response);
        }
    }
}
