using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactPoints;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for unit profile contact point API endpoints for internal consumption (e.g. Notifications) requiring neither authenticated user token nor access token authorization.
    /// </summary>
    [Route("profile/api/v1/units/contactpoint")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UnitContactPointController : ControllerBase
    {
        private readonly IUnitContactPoints _contactPointService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitContactPointController"/> class.
        /// </summary>
        public UnitContactPointController(IUnitContactPoints contactPointsService)
        {
            _contactPointService = contactPointsService;
        }

        /// <summary>
        /// Endpoint looking up the contact points for the units provided in the lookup object in the request body
        /// </summary>
        /// <returns>Returns an overview of the user registered contact points for the provided units and given resource</returns>
        [HttpPost("lookup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UnitContactPointsList>> PostLookup([FromBody] UnitContactPointLookup unitContactPointLookup)
        {     
            Result<UnitContactPointsList, bool> result = await _contactPointService.GetUserRegisteredContactPoints(unitContactPointLookup);
            return result.Match<ActionResult<UnitContactPointsList>>(
                         success => Ok(success),
                         _ => Problem("Could not retrieve contact points"));
        }
    }
}
