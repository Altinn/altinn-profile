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
    /// <remarks>
    /// Initializes a new instance of the <see cref="OrganizationsController"/> class.
    /// </remarks>
    [Route("profile/api/v1/organizations/{organizationNumber}/notificationaddresses")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class OrganizationsController(IOrganizationNotificationAddressesService notificationAddressService) : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService = notificationAddressService;

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

            var response = OrganizationResponseMapper.ToOrganizationResponse(organization);

            return Ok(response);
        }

        /// <summary>
        /// Endpoint looking up a specific notification address for the given organization
        /// </summary>
        /// <returns>Returns the notification addresses for the provided organization</returns>
        [HttpGet("mandatory/{notificationAddressId}")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationAddressResponse>> GetMandatoryNotificationAddress([FromRoute] string organizationNumber, [FromRoute] int notificationAddressId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

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
            var notificationAddress = organization.NotificationAddresses.FirstOrDefault(n => n.NotificationAddressID == notificationAddressId);

            if (notificationAddress == null)
            {
                return NotFound();
            }

            var response = OrganizationResponseMapper.ToNotificationAddressResponse(notificationAddress);

            return Ok(response);
        }

        /// <summary>
        /// Create a new notification address for an organization
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the given organization</returns>
        [HttpPost("mandatory")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Write)]
        [ProducesResponseType(typeof(NotificationAddressResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<NotificationAddressResponse>> CreateNotificationAddress([FromRoute] string organizationNumber, [FromBody] NotificationAddressModel request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(organizationNumber))
            {
                return BadRequest("Organization number is required");
            }

            var notificationAddress = NotificationAddressRequestMapper.ToInternalModel(request);

            var newNotificationAddress = await _notificationAddressService.CreateNotificationAddress(organizationNumber, notificationAddress, cancellationToken);

            var response = OrganizationResponseMapper.ToNotificationAddressResponse(newNotificationAddress);

            return CreatedAtAction(nameof(GetMandatoryNotificationAddress), new { organizationNumber, newNotificationAddress.NotificationAddressID }, response);
        }

        /// <summary>
        /// Update a notification address for an organization
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the given organization</returns>
        [HttpPut("mandatory/{notificationAddressId}")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Write)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationAddressResponse>> UpdateNotificationAddress([FromRoute] string organizationNumber, [FromRoute] int notificationAddressId, [FromBody] NotificationAddressModel request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(organizationNumber))
            {
                return BadRequest("Organization number is required");
            }

            var notificationAddresses = NotificationAddressRequestMapper.ToInternalModel(request, notificationAddressId);

            var updatedNotificationAddress = await _notificationAddressService.UpdateNotificationAddress(organizationNumber, notificationAddresses, cancellationToken);

            if (updatedNotificationAddress == null)
            {
                return NotFound();
            }

            var response = OrganizationResponseMapper.ToNotificationAddressResponse(updatedNotificationAddress);

            return Ok(response);
        }
    }
}
