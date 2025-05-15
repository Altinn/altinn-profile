using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Authorization;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Models;
using AltinnCore.Authentication.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organizing a users favorite parties
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FavoritesController"/> class.
    /// </remarks>
    [Authorize]
    [Route("profile/api/v1/groups/favorites")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class FavoritesController(IPartyGroupService partyGroupService) : ControllerBase
    {
        private readonly IPartyGroupService _partyGroupService = partyGroupService;

        /// <summary>
        /// Endpoint looking up the notification addresses for the given organization
        /// </summary>
        /// <returns>Returns an overview of the registered notification addresses for the provided organization</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrganizationResponse>> Get(CancellationToken cancellationToken)
        {
            string userIdString = Request.HttpContext.User.Claims
                .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
                .Select(c => c.Value).SingleOrDefault();

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Invalid request context. UserId must be provided in claims.");
            }

            int userId = int.Parse(userIdString);

            var favorites = await _partyGroupService.GetFavorites(userId, cancellationToken);

            return Ok(favorites);
        }
    }
}
