using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Authorization;
using Altinn.Profile.Core.User.PartyGroups;
using Altinn.Profile.Models;
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
            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
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
        /// <response code="201">Returns status code 201 if the party is added to favorites</response>
        /// <response code="204">Returns status code 204 if the party is already in the favorites</response>
        [HttpPut("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AddFavorite([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (partyUuid == Guid.Empty)
            {
                return BadRequest("Party UUID cannot be empty.");
            }

            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var addedNow = await _partyGroupService.AddPartyToFavorites(userId, partyUuid, cancellationToken);

            if (addedNow)
            {
                return Created($"profile/api/v1/users/current/party-groups/favorites/{partyUuid}", null);
            }

            return NoContent();
        }

        /// <summary>
        /// Remove a party from the group of favorites for the current user
        /// </summary>
        /// <response code="204">Returns status code 204 if the party was deleted from favorites</response>
        /// <response code="404">Returns status code 404 if the party was not found in favorites</response>
        [HttpDelete("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteFavorite([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (partyUuid == Guid.Empty)
            {
                return BadRequest("Party UUID cannot be empty.");
            }

            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var deletedNow = await _partyGroupService.DeleteFromFavorites(userId, partyUuid, cancellationToken);

            if (!deletedNow)
            {
                return NotFound("Party not found in favorites.");
            }

            return NoContent();
        }
    }
}
