using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Authorization;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organizing the notification addresses a user has registered for parties
    /// </summary>
    [Authorize]
    [Route("profile/api/v1/users/current/notificationsettings")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class NotificationsSettingsController : ControllerBase
    {
        private readonly IProfessionalNotificationsService _professionalNotificationsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsSettingsController"/> class.
        /// </summary>
        public NotificationsSettingsController(IProfessionalNotificationsService professionalNotificationsService)
        {
             _professionalNotificationsService = professionalNotificationsService;
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for a party
        /// </summary>
        [HttpGet("parties/{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProfessionalNotificationAddressResponse>> Get([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
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

            if (partyUuid == Guid.Empty)
            {
                return BadRequest("Party UUID cannot be empty.");
            }

            var notificationAddress = await _professionalNotificationsService.GetNotificationAddresses(userId, partyUuid, cancellationToken);

            if (notificationAddress == null)
            {
                return NotFound("Notification addresses not found for the specified user and party.");
            }

            var response = new ProfessionalNotificationAddressResponse
            {
                UserId = notificationAddress.UserId,
                PartyUuid = notificationAddress.PartyUuid,
                EmailAddress = notificationAddress.EmailAddress,
                PhoneNumber = notificationAddress.PhoneNumber,
                ResourceIncludeList = notificationAddress.GetResourceIncludeList(),
            };

            return Ok(response);
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for a party
        /// </summary>
        [HttpPut("parties/{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromRoute] Guid partyUuid, [FromBody] ProfessionalNotificationAddressRequest request, CancellationToken cancellationToken)
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

            if (partyUuid == Guid.Empty)
            {
                return BadRequest("Party UUID cannot be empty.");
            }

            var userPartyContactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
                UserPartyContactInfoResources = request.ResourceIncludeList?.Select(resource => new UserPartyContactInfoResource
                {
                    ResourceId = resource
                }).ToList()
            };
            var added = await _professionalNotificationsService.AddOrUpdateNotificationAddressesAsync(userPartyContactInfo, cancellationToken);

            if (added)
            {
                return Created($"profile/api/v1/users/current/notificationsettings/parties/{partyUuid}", null);
            }

            return NoContent();
        }
    }
}
