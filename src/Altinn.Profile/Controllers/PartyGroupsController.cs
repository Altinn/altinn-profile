using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for retrieving all groups for the current user
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PartyGroupsController"/> class.
    /// </remarks>
    [Authorize]
    [Route("profile/api/v1/users/current/party-groups")]
    [Produces("application/json")]
    public class PartyGroupsController(IPartyGroupService partyGroupService) : ControllerBase
    {
        private readonly IPartyGroupService _partyGroupService = partyGroupService;

        /// <summary>
        /// Retrieve all groups for a user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>All groups for the current user.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<GroupResponse>>> Get(CancellationToken cancellationToken)
        {
            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var groupResponse = await _partyGroupService.GetGroupsForAUser(userId, cancellationToken);

            var response = groupResponse.Select(g => new GroupResponse
            {
                Parties = [.. g.Parties.Select(p => p.PartyUuid)],
                Name = g.Name,
                IsFavorite = g.IsFavorite,
                GroupId = g.GroupId
            });

            return Ok(response);
        }

        /// <summary>
        /// Create a new group for the user
        /// </summary>
        /// <param name="request">The group creation request containing the group name</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The created group.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupResponse>> Create([FromBody]GroupRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var groupResponse = await _partyGroupService.CreateGroup(userId, request.Name, cancellationToken);

            var response = new GroupResponse
            {
                Parties = [.. groupResponse.Parties.Select(p => p.PartyUuid)],
                Name = groupResponse.Name,
                IsFavorite = groupResponse.IsFavorite,
                GroupId = groupResponse.GroupId
            };

            return Created($"/profile/api/v1/users/current/party-groups/{response.GroupId}", response);
        }
    }
}
