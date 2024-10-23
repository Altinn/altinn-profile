using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller for user profile contact point API endpoints for internal consumption (e.g. Notifications) requiring neither authenticated user token nor access token authorization.
/// </summary>
[ApiController]
[Route("profile/api/v1/users/contactpoint")]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
public class UserContactPointController : ControllerBase
{
    private readonly IUserContactPoints _contactPointService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointController"/> class.
    /// </summary>
    public UserContactPointController(IUserContactPoints contactPointService)
    {
        _contactPointService = contactPointService;
    }

    /// <summary>
    /// Endpoint looking up the availability of contact points for the provideded national identity number in the request body
    /// </summary>
    /// <returns>Returns an overview of the availability of various contact points for the user</returns>
    [HttpPost("availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserContactPointAvailabilityList>> PostAvailabilityLookup([FromBody] UserContactDetailsLookupCriteria userContactPointLookup)
    {
        if (userContactPointLookup.NationalIdentityNumbers.Count == 0)
        {
            return new UserContactPointAvailabilityList();
        }

        Result<UserContactPointAvailabilityList, bool> result = await _contactPointService.GetContactPointAvailability(userContactPointLookup.NationalIdentityNumbers);

        return result.Match<ActionResult<UserContactPointAvailabilityList>>(
                       success => Ok(success),
                       _ => Problem("Could not retrieve contact point availability"));
    }

    /// <summary>
    /// Endpoint looking up the contact points for the user connected to the provideded national identity number in the request body
    /// </summary>
    /// <returns>Returns an overview of the contact points for the user</returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserContactPointsList>> PostLookup([FromBody] UserContactDetailsLookupCriteria userContactPointLookup)
    {
        Result<UserContactPointsList, bool> result = await _contactPointService.GetContactPoints(userContactPointLookup.NationalIdentityNumbers);
        return result.Match<ActionResult<UserContactPointsList>>(
                     success => Ok(success),
                     _ => Problem("Could not retrieve contact points"));
    }
}
