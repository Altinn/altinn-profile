using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organization contact point API endpoints for internal consumption (e.g. Notifications) requiring neither authenticated user token nor access token authorization.
    /// </summary>
    [Route("profile/api/v1/organizations/contactpoint")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class OrgContactPointController : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrgContactPointController"/> class.
        /// </summary>
        public OrgContactPointController(IOrganizationNotificationAddressesService notificationAddressService)
        {
            _notificationAddressService = notificationAddressService;
        }

        /// <summary>
        /// Endpoint looking up the contact points for the organization provided in the lookup object in the request body
        /// </summary>
        /// <returns>Returns an overview of the user registered contact points for the provided organization</returns>
        [HttpPost("lookup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrgContactPointsList>> PostLookup([FromBody] OrgContactPointLookup orgContactPointLookup, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result<OrgContactPointsList, bool> result = await _notificationAddressService.GetNotificationContactPoints(orgContactPointLookup, cancellationToken);
            return result.Match<ActionResult<OrgContactPointsList>>(
                         success => Ok(success),
                         _ => Problem("Could not retrieve contact points"));
        }
    }
}
