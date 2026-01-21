using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

    /// <summary>
    /// Updates the profile settings of the current user based on the request context
    /// </summary>
    /// <returns>User profile of current user</returns>
    [HttpPut("current/profilesettings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileSettingPreference>> UpdateProfileSettings([FromBody]ProfileSettingPutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string userIdString = Request.HttpContext.User.Claims
            .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
            .Select(c => c.Value).SingleOrDefault();

        if (string.IsNullOrEmpty(userIdString))
        {
            return BadRequest("Invalid request context. UserId must be provided in claims.");
        }

        int userId = int.Parse(userIdString);

        var profileSettings = new ProfileSettings
        {
            UserId = userId,
            LanguageType = request.Language,
            DoNotPromptForParty = request.DoNotPromptForParty.Value,
            PreselectedPartyUuid = request.PreselectedPartyUuid,
            ShowClientUnits = request.ShowClientUnits.Value,
            ShouldShowSubEntities = request.ShouldShowSubEntities.Value,
            ShouldShowDeletedEntities = request.ShouldShowDeletedEntities.Value
        };
        var userProfileSettings = await _userProfileService.UpdateProfileSettings(profileSettings, cancellationToken);

        var profileSettingsPreference = new ProfileSettingPreference
        {
            Language = userProfileSettings.LanguageType,
            DoNotPromptForParty = userProfileSettings.DoNotPromptForParty,
            PreselectedPartyUuid = userProfileSettings.PreselectedPartyUuid,
            ShowClientUnits = userProfileSettings.ShowClientUnits,
            ShouldShowSubEntities = userProfileSettings.ShouldShowSubEntities,
            ShouldShowDeletedEntities = userProfileSettings.ShouldShowDeletedEntities,
        };

        return Ok(profileSettingsPreference);
    }

    /// <summary>
    /// Updates the profile settings of the current user based on the request context
    /// </summary>
    /// <returns>User profile of current user</returns>
    [HttpPatch("current/profilesettings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileSettingPreference>> PatchProfileSettings([FromBody] ProfileSettingsPatchRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string userIdString = Request.HttpContext.User.Claims
            .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
            .Select(c => c.Value).SingleOrDefault();

        if (string.IsNullOrEmpty(userIdString))
        {
            return BadRequest("Invalid request context. UserId must be provided in claims.");
        }

        int userId = int.Parse(userIdString);

        var patchModel = new ProfileSettingsPatchModel
        {
            UserId = userId,
            Language = request.Language,
            DoNotPromptForParty = request.DoNotPromptForParty,
            PreselectedPartyUuid = request.PreselectedPartyUuid,
            ShowClientUnits = request.ShowClientUnits,
            ShouldShowSubEntities = request.ShouldShowSubEntities,
            ShouldShowDeletedEntities = request.ShouldShowDeletedEntities
        };

        var userProfileSettings = await _userProfileService.PatchProfileSettings(patchModel, cancellationToken);

        if (userProfileSettings == null)
        {
            return NotFound();
        }

        var profileSettingsPreference = new ProfileSettingPreference
        {
            Language = userProfileSettings.LanguageType,
            DoNotPromptForParty = userProfileSettings.DoNotPromptForParty,
            PreselectedPartyUuid = userProfileSettings.PreselectedPartyUuid,
            ShowClientUnits = userProfileSettings.ShowClientUnits,
            ShouldShowSubEntities = userProfileSettings.ShouldShowSubEntities,
            ShouldShowDeletedEntities = userProfileSettings.ShouldShowDeletedEntities,
        };

        return Ok(profileSettingsPreference);
    }
}
