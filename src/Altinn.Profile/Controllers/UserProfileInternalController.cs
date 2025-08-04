using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core;
using Altinn.Profile.Core.User;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller for user profile API endpoints for internal consumption (e.g. Authorization) requiring neither authenticated user token nor access token authorization.
/// </summary>
[Route("profile/api/v1/internal/user")]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
public class UserProfileInternalController : Controller
{
    private readonly IUserProfileService _userProfileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileInternalController"/> class
    /// </summary>
    /// <param name="userProfileService">The user profile service</param>
    public UserProfileInternalController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Gets the user profile for a given user identified by one of the available types of user identifiers:
    ///     UserId (from Altinn 2 Authn UserProfile)
    ///     UserUuid (from Altinn 2 Authn UserProfile)
    ///     Username (from Altinn 2 Authn UserProfile)
    ///     SSN/Dnr (from Freg)
    /// </summary>
    /// <param name="userProfileLookup">Input model for providing one of the supported lookup parameters</param>
    /// <returns>User profile of the given user</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfile>> Get([FromBody] UserProfileLookup userProfileLookup)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Result<UserProfile, bool> result;

        if (userProfileLookup != null && userProfileLookup.UserId.HasValue && userProfileLookup.UserId != 0)
        {
            result = await _userProfileService.GetUser((int)userProfileLookup.UserId);
        }
        else if (userProfileLookup?.UserUuid != null)
        {
            result = await _userProfileService.GetUserByUuid(userProfileLookup.UserUuid.Value);
        }
        else if (!string.IsNullOrWhiteSpace(userProfileLookup?.Username))
        {
            result = await _userProfileService.GetUserByUsername(userProfileLookup.Username);
        }
        else if (!string.IsNullOrWhiteSpace(userProfileLookup?.Ssn))
        {
            result = await _userProfileService.GetUser(userProfileLookup.Ssn);
        }
        else
        {
            return BadRequest();
        }

        return result.Match<ActionResult<UserProfile>>(
              userProfile => Ok(userProfile),
              _ => NotFound());
    }

    /// <summary>
    /// Gets a list of user profiles for a list of of users identified by userUuid.
    /// </summary>
    /// <param name="userUuidList">List of uuid identifying the users profiles to return</param>
    /// <returns>List of user profiles</returns>
    [HttpPost]
    [Route("listbyuuid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<List<UserProfile>>> GetList([FromBody] List<Guid> userUuidList)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userUuidList == null || userUuidList.Count == 0)
        {
            return BadRequest();
        }

        Result<List<UserProfile>, bool> result = await _userProfileService.GetUserListByUuid(userUuidList);
        List<UserProfile> userProfiles = result.Match(
             userProfileList => userProfileList,
             _ => []);
             
        return Ok(userProfiles);
    }
}
