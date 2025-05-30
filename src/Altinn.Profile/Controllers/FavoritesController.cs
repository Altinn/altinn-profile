using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    [Route("profile/api/v1/users/current/party-groups/favorites")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class FavoritesController(IPartyGroupService partyGroupService) : ControllerBase
    {
        private readonly IPartyGroupService _partyGroupService = partyGroupService;

        /// <summary>
        /// Get the favorite parties for the current user
        /// </summary>
        /// <returns>Returns the group containing the favorite parties for current user</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GroupResponse>> Get(CancellationToken cancellationToken)
        {
            var validationResult = TryGetUserIdFromClaims(out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var favorites = await _partyGroupService.GetFavorites(userId, cancellationToken);

            var response = new GroupResponse { Parties = [.. favorites.Parties.Select(p => p.PartyUuid)], Name = favorites.Name, IsFavorite = favorites.IsFavorite };
            return Ok(response);
        }

        /// <summary>
        /// Add a party to the group of favorites for the current user
        /// </summary>
        [HttpPut("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GroupResponse>> AddFavorite([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (partyUuid == Guid.Empty)
            {
                return BadRequest("Invalid party UUID provided.");
            }

            var validationResult = TryGetUserIdFromClaims(out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var addedNow = await _partyGroupService.MarkPartyAsFavorite(userId, partyUuid, cancellationToken);

            if (addedNow)
            {
                return Created((string)null, null);
            }

            return NoContent();
        }

        private ObjectResult TryGetUserIdFromClaims(out int userId)
        {
            userId = 0;
            string userIdString = Request.HttpContext.User.Claims
                .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
                .Select(c => c.Value).SingleOrDefault();

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Invalid request context. UserId must be provided in claims.");
            }

            if (!int.TryParse(userIdString, out userId))
            {
                return BadRequest("Invalid user ID format in claims.");
            }

            return null; // Success case
        }
    }
}
