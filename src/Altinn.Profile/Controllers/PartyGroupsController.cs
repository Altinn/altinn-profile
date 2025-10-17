using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// Controller for retrieving all groups for a party
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PartyGroupsController"/> class.
    /// </remarks>
    [Authorize]
    [Route("profile/api/v1/users/current/party-groups")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class PartyGroupsController(IPartyGroupService partyGroupService) : ControllerBase
    {
        private readonly IPartyGroupService _partyGroupService = partyGroupService;

        /// <summary>
        /// Retrieve all groups for a user
        /// </summary>
        /// <returns>Returns the group containing the favorite parties for current user</returns>
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
         
            var partyGroupsForAUser = groupResponse
                                .GroupBy(g => new { g.GroupId, g.Name, g.IsFavorite })
                                .Select(grouping => new
                                {
                                    grouping.Key.GroupId,
                                    grouping.Key.Name,
                                    grouping.Key.IsFavorite,
                                    PartyUuids = grouping
                                        .SelectMany(g => g.Parties)
                                        .Select(p => p.PartyUuid)
                                        .ToList()
                                })
                                .ToList();

            return Ok(partyGroupsForAUser);
        }
    }
}
