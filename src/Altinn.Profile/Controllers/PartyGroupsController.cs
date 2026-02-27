using System;
using System.Collections.Generic;
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
        /// <response code="200">The group name was successfully updated.</response>
        /// <response code="400">The request is invalid (e.g., missing or empty name).</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="404">The group does not exist or the user does not have access to it.</response>
        /// <response code="422">The group exists but cannot be renamed because it is a favorite group. Favorite groups have a system-managed name and cannot be modified.</response>
        [HttpPatch("{groupId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
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

            var result = await _partyGroupService.UpdateGroupName(userId, groupId, request.Name, cancellationToken);

            return result.Result switch
            {
                GroupOperationResult.Success => Ok(MapToGroupResponse(result.Group!)),
                GroupOperationResult.NotFound => NotFound(),
                GroupOperationResult.Forbidden => UnprocessableEntity("Favorite groups cannot be renamed."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        /// <param name="groupId">The ID of the group to delete</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>NoContent if successful.</returns>
        /// <response code="204">The group was successfully deleted.</response>
        /// <response code="400">The request is invalid (e.g., the groupId is not a valid integer).</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="404">The group does not exist or the user does not have access to it.</response>
        /// <response code="422">The group exists but cannot be deleted because it is a favorite group. Favorite groups are system-managed and cannot be deleted by users.</response>
        [HttpDelete("{groupId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Delete([FromRoute] int groupId, CancellationToken cancellationToken)
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

            var result = await _partyGroupService.DeleteGroup(userId, groupId, cancellationToken);

            return result switch
            {
                GroupOperationResult.Success => NoContent(),
                GroupOperationResult.NotFound => NotFound(),
                GroupOperationResult.Forbidden => UnprocessableEntity("Favorite groups cannot be deleted."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        /// <summary>
        /// Add a party to a group
        /// </summary>
        /// <param name="groupId">The ID of the group to add the party to</param>
        /// <param name="partyUuid">The UUID of the party to add</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The updated group.</returns>
        /// <response code="200">The party was successfully added to the group. Returns the updated group. If the party is already in the group, the operation is idempotent and returns the current state.</response>
        /// <response code="400">The request is invalid (e.g., the groupId is not a valid integer, or the partyUuid is not a valid GUID).</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="404">The group does not exist or the user does not have access to it.</response>
        [HttpPut("{groupId:int}/associations/{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupResponse>> AddPartyToGroup([FromRoute] int groupId, [FromRoute] Guid partyUuid, CancellationToken cancellationToken)
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

            var group = await _partyGroupService.AddPartyToGroup(userId, groupId, partyUuid, cancellationToken);

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
