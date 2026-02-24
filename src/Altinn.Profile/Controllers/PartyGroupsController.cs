using System;
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
        public async Task<ActionResult<GroupResponse>> Get([FromRoute]int groupId, CancellationToken cancellationToken)
        {
            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var groupResponse = await _partyGroupService.GetGroup(userId, groupId, cancellationToken);

            if (groupResponse == null)
            {
                return NotFound();
            }

            var response = new GroupResponse
            {
                Parties = [.. groupResponse.Parties.Select(p => p.PartyUuid)],
                Name = groupResponse.Name,
                IsFavorite = groupResponse.IsFavorite,
                GroupId = groupResponse.GroupId
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieve all groups for a user
        /// </summary>
        /// <returns>All groups for the current user.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    }
}
