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
        /// Create a new notification address for an organization. 
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the given organization</returns>
        /// <response code="201">Returns the newly created notification address</response>
        /// <response code="200">Returns the existing address if it is already registered. This means that duplicate create commands will only result in the creation one notification address.</response>
        [HttpPost("mandatory")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Write)]
        [ProducesResponseType(typeof(NotificationAddressResponse), StatusCodes.Status200OK)]
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
                return Problem("Organization number is required", statusCode: 400);
            }

            var notificationAddress = NotificationAddressRequestMapper.ToInternalModel(request);

            var (newNotificationAddress, isNew) = await _notificationAddressService.CreateNotificationAddress(organizationNumber, notificationAddress, cancellationToken);

            var response = OrganizationResponseMapper.ToNotificationAddressResponse(newNotificationAddress);

            if (isNew)
            {
                return CreatedAtAction(nameof(GetMandatoryNotificationAddress), new { organizationNumber, newNotificationAddress.NotificationAddressID }, response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Update a notification address for an organization
        /// </summary>
        /// <returns>Returns the updated notification address for the given organization</returns>
        /// <response code="200">Returns the updated address if it is already registered</response>
        /// <response code="409">Returns problem details with a reference to the conflicting address in the instance parameter</response>
        [HttpPut("mandatory/{notificationAddressId}")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Write)]
        [ProducesResponseType(typeof(NotificationAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<NotificationAddressResponse>> UpdateNotificationAddress([FromRoute] string organizationNumber, [FromRoute] int notificationAddressId, [FromBody] NotificationAddressModel request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(organizationNumber))
            {
                return Problem("Organization number is required", statusCode: 400);
            }

            var notificationAddress = NotificationAddressRequestMapper.ToInternalModel(request, notificationAddressId);

            var (updatedNotificationAddress, isDuplicate) = await _notificationAddressService.UpdateNotificationAddress(organizationNumber, notificationAddress, cancellationToken);

            if (updatedNotificationAddress == null)
            {
                return NotFound();
            }

            if (isDuplicate)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = "A notification address with the same address already exists.",
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    Instance = Url.Action(nameof(GetMandatoryNotificationAddress), new { organizationNumber, updatedNotificationAddress.NotificationAddressID })
                });
            }

            var response = OrganizationResponseMapper.ToNotificationAddressResponse(updatedNotificationAddress);

            return Ok(response);
        }
        
          /// <summary>
        /// Delete a notification address for an organization
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the given organization</returns>
        [HttpDelete("mandatory/{notificationAddressId}")]
        [Authorize(Policy = AuthConstants.OrgNotificationAddress_Write)]
        [ProducesResponseType(typeof(NotificationAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationAddressResponse>> DeleteNotificationAddress([FromRoute] string organizationNumber, [FromRoute] int notificationAddressId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(organizationNumber))
            {
                return BadRequest("Organization number is required");
            }

            try
            {
                var updatedNotificationAddress = await _notificationAddressService.DeleteNotificationAddress(organizationNumber, notificationAddressId, cancellationToken);
                if (updatedNotificationAddress == null)
                {
                    return NotFound();
                }

                var response = OrganizationResponseMapper.ToNotificationAddressResponse(updatedNotificationAddress);

                return Ok(response);
            }
            catch (InvalidOperationException ex) 
                when (ex.Message.Equals("Cannot delete the last notification address", StringComparison.InvariantCultureIgnoreCase))
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                };
                return Conflict(problemDetails);
            }
        }
    }
}
