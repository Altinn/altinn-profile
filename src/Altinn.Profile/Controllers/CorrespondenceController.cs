#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller for handling correspondence-related operations.
/// </summary>
[ApiController]
[Authorize]
[Authorize(Policy = AuthConstants.CorrespondenceAccess)]
[Route("profile/api/v1/correspondence")]
[Produces("application/json")]
public class CorrespondenceController : ControllerBase
{
    private readonly IUnitContactPointsService _contactPointsService;
    private readonly IOrganizationNotificationAddressesService _notificationAddressService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrespondenceController"/> class.
    /// </summary>
    /// <param name="contactPointsService">
    /// A service implementation of <see cref="IUnitContactPointsService"/> that handles business logic
    /// related to professional notification addresses.
    /// </param>
    /// <param name="notificationAddressService">
    /// A service implementation of <see cref="IOrganizationNotificationAddressesService"/> that handles business logic
    /// related to organization notification addresses.
    /// </param>
    public CorrespondenceController(IUnitContactPointsService contactPointsService, IOrganizationNotificationAddressesService notificationAddressService)
    {
        _contactPointsService = contactPointsService;
        _notificationAddressService = notificationAddressService;
    }

    /// <summary>
    /// Endpoint for looking up user-registered notification addresses for the provided organizations and the
    /// given resource id.
    /// </summary>
    /// <param name="unitContactPointLookup">The search criteria.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// Returns a list of user-registered notification addresses for the provided units.
    /// </returns>
    [HttpPost("units/contactpoint/lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UnitContactPointsList>> GetUserRegisteredContactPoints(
        [FromBody][Required] UnitContactPointLookup unitContactPointLookup, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        UnitContactPointsList result = await _contactPointsService.GetUserRegisteredContactPoints(
            [.. unitContactPointLookup.OrganizationNumbers], unitContactPointLookup.ResourceId, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Endpoint looking up the notification addresses for the organization provided in the lookup object in the
    /// request body. If the organization has no notification addresses registered, the main unit address will be
    /// returned if it exists.
    /// </summary>
    /// <param name="orgContactPointLookup">The search criteria.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns an overview of the user registered notification addresses for the provided organization</returns>
    [HttpPost("organizations/notificationaddresses/lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrgNotificationAddressesResponse>> GetOrganizationRegisteredContactPoints(
        [FromBody][Required] OrgNotificationAddressRequest orgContactPointLookup, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        IEnumerable<Organization> organizations = 
            await _notificationAddressService.GetOrganizationNotificationAddresses(
                orgContactPointLookup.OrganizationNumbers, cancellationToken, true);

        OrgNotificationAddressesResponse result = OrgNotificationAddressesResponse.Create(organizations);
        return Ok(result);
    }
}
