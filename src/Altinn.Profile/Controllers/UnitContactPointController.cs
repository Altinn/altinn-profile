using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Unit.ContactPoints;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// This controller provides endpoint for accessing user registered notification addresses for organizations.
/// The notification addresses are typically registered by users in a professional context.
/// </summary>
/// <remarks>
/// The endpoints are intended for internal consumption (e.g. Notifications) and do not require authenticated
/// user token or access token authorization.
/// </remarks>
[Route("profile/api/v1/units/contactpoint")]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
public class UnitContactPointController : ControllerBase
{
    private readonly IUnitContactPointsService _contactPointService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitContactPointController"/> class.
    /// </summary>
    public UnitContactPointController(IUnitContactPointsService contactPointsService)
    {
        _contactPointService = contactPointsService;
    }

    /// <summary>
    /// Endpoint for looking up use registered notification addresses for the provided units in the
    /// request body.
    /// </summary>
    /// <returns>
    /// Returns a list of user registered notification addresses for the provided units.
    /// </returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UnitContactPointsList>> PostLookup(
        [FromBody] UnitContactPointLookup unitContactPointLookup)
    {     
        Result<UnitContactPointsList, bool> result = 
            await _contactPointService.GetUserRegisteredContactPoints(unitContactPointLookup);

        return result.Match<ActionResult<UnitContactPointsList>>(
            success => Ok(success),
            _ => Problem("Could not retrieve contact points"));
    }
}
