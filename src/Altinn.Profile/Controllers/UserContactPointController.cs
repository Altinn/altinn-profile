using System;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
    private readonly IUserContactPointsService _contactPointService;
    private readonly ILogger<UserContactPointController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointController"/> class.
    /// </summary>
    public UserContactPointController(IUserContactPointsService contactPointService, ILogger<UserContactPointController> logger)
    {
        _contactPointService = contactPointService;
        _logger = logger;
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

        ActionResult<UserContactPointAvailabilityList> requestResult;

        try
        {
            UserContactPointAvailabilityList result = await _contactPointService.GetContactPointAvailability(userContactPointLookup.NationalIdentityNumbers);
            requestResult = Ok(result);
        }
        catch
        {
            requestResult = Problem("An unexpected error occurred.");
        }

        return requestResult;
    }

    /// <summary>
    /// Endpoint looking up the contact points for the user connected to the provided national identity number in the request body
    /// </summary>
    /// <returns>Returns an overview of the contact points for the user</returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserContactPointsList>> PostLookup([FromBody] UserContactDetailsLookupCriteria userContactPointLookup)
    {
        if (userContactPointLookup.NationalIdentityNumbers.Count == 0)
        {
            return Ok(new UserContactPointsList());
        }

        ActionResult<UserContactPointsList> requestResult;

        try
        {
            UserContactPointsList userContactPointsList = await _contactPointService.GetContactPoints(userContactPointLookup.NationalIdentityNumbers);
            requestResult = Ok(userContactPointsList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details.");
            requestResult = Problem("An unexpected error occurred.");
        }
        
        return requestResult;
    }
}
