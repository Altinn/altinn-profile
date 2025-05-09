using System;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Authorization;
using Altinn.Profile.Core;
using Altinn.Profile.Core.User;

using AltinnCore.Authentication.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller for all operations related to users
/// </summary>
[Authorize]
[Route("profile/api/v1/users")]
[Consumes("application/json")]
[Produces("application/json")]
public class UsersController : Controller
{
    private readonly IUserProfileService _userProfileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class
    /// </summary>
    /// <param name="userProfileService">The user profile service</param>
    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Gets the user profile for a given user id
    /// </summary>
    /// <param name="userID">The user id</param>
    /// <returns>The information about a given user</returns>
    [HttpGet("{userID:int}")]
    [Authorize(Policy = AuthConstants.PlatformAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfile>> Get(int userID)
    {
        if (userID == 0)
        {
            return NotFound();
        }

        Result<UserProfile, bool> result = await _userProfileService.GetUser(userID);

        return result.Match<ActionResult<UserProfile>>(
            userProfile => Ok(userProfile),
            _ => NotFound());
    }

    /// <summary>
    /// Gets the user profile for a given user uuid
    /// </summary>
    /// <param name="userUuid">The user uuid</param>
    /// <returns>The information about a given user</returns>
    [HttpGet("byuuid/{userUuid:Guid}")]
    [Authorize(Policy = AuthConstants.PlatformAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfile>> Get([FromRoute] Guid userUuid)
    {
        Result<UserProfile, bool> result = await _userProfileService.GetUserByUuid(userUuid);

        return result.Match<ActionResult<UserProfile>>(
            userProfile => Ok(userProfile),
            _ => NotFound());
    }

    /// <summary>
    /// Gets the current user based on the request context
    /// </summary>
    /// <returns>User profile of current user</returns>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfile>> Get()
    {
        string userIdString = Request.HttpContext.User.Claims
            .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
            .Select(c => c.Value).SingleOrDefault();

        if (string.IsNullOrEmpty(userIdString))
        {
            return BadRequest("Invalid request context. UserId must be provided in claims.");
        }

        int userId = int.Parse(userIdString);

        return await Get(userId);
    }

    /// <summary>
    /// Gets the user profile for a given SSN
    /// </summary>
    /// <param name="ssn">The user's social security number</param>
    /// <returns>User profile connected to given SSN </returns>
    [HttpPost]
    [Authorize(Policy = AuthConstants.PlatformAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfile>> GetUserFromSSN([FromBody] string ssn)
    {
        Result<UserProfile, bool> result = await _userProfileService.GetUser(ssn);

        return result.Match<ActionResult<UserProfile>>(
            userProfile => Ok(userProfile),
            _ => NotFound());
    }
}
