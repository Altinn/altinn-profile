using System.Threading.Tasks;
using Altinn.Platform.Profile.Models;
using Altinn.Profile.Models;
using Altinn.Profile.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for user profile API endpoints for internal consumption (e.g. Authorization) requiring neither authenticated user token nor access token authorization.
    /// </summary>
    [Route("profile/api/v1/internal/user")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UserProfileInternalController : Controller
    {
        private readonly IUserProfiles _userProfilesWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileInternalController"/> class
        /// </summary>
        /// <param name="userProfilesWrapper">The users wrapper</param>
        public UserProfileInternalController(IUserProfiles userProfilesWrapper)
        {
            _userProfilesWrapper = userProfilesWrapper;
        }

        /// <summary>
        /// Gets the user profile for a given user identified by one of the available types of user identifiers:
        ///     UserId (from Altinn 2 Authn UserProfile)
        ///     Username (from Altinn 2 Authn UserProfile)
        ///     SSN/Dnr (from Freg)
        ///     Uuid (from Altinn 2 Party/UserProfile implementation will be added later)
        /// </summary>
        /// <param name="userProfileLookup">Input model for providing one of the supported lookup parameters</param>
        /// <returns>User profile of the given user</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfile>> Get([FromBody] UserProfileLookup userProfileLookup)
        {
            UserProfile result;
            if (userProfileLookup != null && userProfileLookup.UserId != 0)
            {
                result = await _userProfilesWrapper.GetUser(userProfileLookup.UserId);
            }
            else if (!string.IsNullOrWhiteSpace(userProfileLookup?.Username))
            {
                result = await _userProfilesWrapper.GetUserByUsername(userProfileLookup.Username);
            }
            else if (!string.IsNullOrWhiteSpace(userProfileLookup?.Ssn))
            {
                result = await _userProfilesWrapper.GetUser(userProfileLookup.Ssn);
            }
            else
            {
                return BadRequest();
            }

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}