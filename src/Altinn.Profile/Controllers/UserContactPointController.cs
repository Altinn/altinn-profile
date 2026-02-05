using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Models;
using Altinn.Urn;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller for user profile contact point API endpoints for internal consumption (e.g. Notifications) requiring neither authenticated user token nor access token authorization.
/// </summary>
[ApiController]
[Route("profile/api/v1/users/contactpoint")]
[ApiExplorerSettings(IgnoreApi = false)]
[Consumes("application/json")]
[Produces("application/json")]
public class UserContactPointController : ControllerBase
{
    private readonly IUserContactPointsService _contactPointService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointController"/> class.
    /// </summary>
    public UserContactPointController(IUserContactPointsService contactPointService)
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

        UserContactPointAvailabilityList result = await _contactPointService.GetContactPointAvailability(userContactPointLookup.NationalIdentityNumbers);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint looking up the contact points for the user connected to the provided national identity number in the request body  
    /// </summary>
    /// <returns>Returns an overview of the contact points for the user</returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserContactPointsList>> PostLookup([FromBody] UserContactDetailsLookupCriteria userContactPointLookup, CancellationToken cancellationToken)
    {
        if (userContactPointLookup.NationalIdentityNumbers.Count == 0)
        {
            return Ok(new UserContactPointsList());
        }
 
        UserContactPointsList userContactPointsList = await _contactPointService.GetContactPoints(userContactPointLookup.NationalIdentityNumbers, cancellationToken);
        return Ok(userContactPointsList);
    }

    /// <summary>
    /// Endpoint used for looking up contact points for self-identified users connected to the provided email identifiers in the request body
    /// </summary>
    /// <param name="selfIdentifiedContactPointLookup">Request body object that contains the identifiers associated with self-identified users</param>
    /// <param name="cancellationToken">A cancellationToken used to cancel the asynchronous operation</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("lookupsi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SelfIdentifiedUserContactPointsList>> PostLookupSi([FromBody] SelfIdentifiedContactDetailsLookupCriteria selfIdentifiedContactPointLookup, CancellationToken cancellationToken)
    {
        if (selfIdentifiedContactPointLookup.ExternalIdentities.Count == 0)
        {
            return Ok(new SelfIdentifiedUserContactPointsList());
        }

        var response = await _contactPointService.GetSiContactPoints(selfIdentifiedContactPointLookup.ExternalIdentities, cancellationToken);
        return Ok(response);
    }
}
