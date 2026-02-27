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
        /// Retrieve a specific group for a user
        /// </summary>
        /// <param name="groupId">The ID of the group to retrieve</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The group with that specific id for the current user.</returns>
        [HttpGet("{groupId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupResponse>> Get([FromRoute] int groupId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var group = await _partyGroupService.GetGroup(userId, groupId, cancellationToken);

            if (group == null)
            {
                return NotFound();
            }

            var response = MapToGroupResponse(group);

            return Ok(response);
        }

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

            var response = groupResponse.Select(MapToGroupResponse);

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

            var response = MapToGroupResponse(groupResponse);

            return Created($"/profile/api/v1/users/current/party-groups/{response.GroupId}", response);
        }

        /// <summary>
        /// Update the name of an existing group
        /// </summary>
        /// <param name="groupId">The ID of the group to update</param>
        /// <param name="request">The group update request containing the new group name</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The updated group.</returns>
        [HttpPatch("{groupId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupResponse>> UpdateName([FromRoute] int groupId, [FromBody] GroupRequest request, CancellationToken cancellationToken)
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

            var group = await _partyGroupService.UpdateGroupName(userId, groupId, request.Name, cancellationToken);

            if (group == null)
            {
                return NotFound();
            }

            var response = MapToGroupResponse(group);

            return Ok(response);
        }

        private GroupResponse MapToGroupResponse(Group group)
        {
            return new GroupResponse
            {
                Parties = [.. group.Parties.Select(p => p.PartyUuid)],
                Name = group.Name,
                IsFavorite = group.IsFavorite,
                GroupId = group.GroupId
            };
        }
    }
}
