using System;
using System.Collections.Generic;
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
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </remarks>
    [Authorize(Policy = AuthConstants.SupportDashboardAccess)]
    [Route("profile/api/v1/dashboard")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DashboardController(IOrganizationNotificationAddressesService notificationAddressService) : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService = notificationAddressService;

        /// <summary>
        /// Endpoint that can retrieve a list of all Notification Addresses for the given organization
        /// </summary>
        /// <returns>Returns the notification addresses for the provided organization</returns>                
        [HttpGet("organizations/{organizationNumber}/notificationaddresses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardNotificationAddressResponse>>> GetAllNotificationAddressesForAnOrg([FromRoute] string organizationNumber, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddresses([organizationNumber], cancellationToken, true);

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
            var notificationAddresses = organization.NotificationAddresses;

            if (notificationAddresses == null)
            {
                return NotFound();
            }

            var addresses = FilterAndMapAddresses(organizations);

            return Ok(addresses);
        }

        /// <summary>
        /// Endpoint that can retrieve a list of all Notification Addresses for the given email address
        /// </summary>
        /// <returns>Returns the notification addresses for the provided email address</returns>                
        [HttpGet("organizations/notificationaddresses/email/{emailAddress}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardNotificationAddressResponse>>> GetNotificationAddressesByEmailAddress([FromRoute] string phoneNumber, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddressesByPhoneNumber(phoneNumber, cancellationToken);

            var orgCount = organizations.Count();

            if (orgCount == 0)
            {
                return NotFound();
            }            

            var addresses = FilterAndMapAddresses(organizations);

            return Ok(addresses);
        }

        /// <summary>
        /// Endpoint that can retrieve a list of all Notification Addresses for the given email address
        /// </summary>
        /// <returns>Returns the notification addresses for the provided email address</returns>                
        [HttpGet("organizations/notificationaddresses/phone/{phoneNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardNotificationAddressResponse>>> GetNotificationAddressesByPhoneNumber([FromRoute] string phoneNumber, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddressesByPhoneNumber(phoneNumber, cancellationToken);

            var orgCount = organizations.Count();

            if (orgCount == 0)
            {
                return NotFound();
            }            

            var addresses = FilterAndMapAddresses(organizations);

            return Ok(addresses);
        }

        private static List<DashboardNotificationAddressResponse> FilterAndMapAddresses(IEnumerable<Organization> organizations)
        {
            var allAddresses = new List<DashboardNotificationAddressResponse>();

            foreach (var organization in organizations)
            {
                if (organization.NotificationAddresses == null)
                {
                    continue;
                }

                var addresses = organization.NotificationAddresses
                    .Where(n => n.IsSoftDeleted != true && n.HasRegistryAccepted != false)
                    .Select(n => OrganizationResponseMapper.ToDashboardNotificationAddressResponse(
                        n,
                        requestedOrgNumber: organization.OrganizationNumber,
                        sourceOrgNumber: organization.AddressOrigin))
                    .ToList();

                allAddresses.AddRange(addresses);
            }

            return allAddresses;
        }
    }
}
