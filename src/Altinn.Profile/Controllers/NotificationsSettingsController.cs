using System;
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
    /// Controller for organizing a users favorite parties
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NotificationsSettingsController"/> class.
    /// </remarks>
    [Authorize]
    [Route("profile/api/v1/users/current/notificationsettings")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class NotificationsSettingsController(IProfessionalNotificationsService professionalNotificationsService) : ControllerBase
    {
        private readonly IProfessionalNotificationsService _professionalNotificationsService = professionalNotificationsService;

        /// <summary>
        /// Get the notification addresses the current user has registered for a party
        /// </summary>
        [HttpGet("parties/{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProfessionalNotificationAddresses>> Get([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
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

            var notificationAddress = await _professionalNotificationsService.GetNotificationAddresses(userId, partyUuid, cancellationToken);

            if (notificationAddress == null)
            {
                return NotFound(new { Message = "Notification addresses not found for the specified user and party." });
            }

            var response = new ProfessionalNotificationAddresses
            {
                UserId = notificationAddress.UserId,
                PartyUuid = notificationAddress.PartyUuid,
                EmailAddress = notificationAddress.EmailAddress,
                PhoneNumber = notificationAddress.PhoneNumber,
                ResourceIncludeList = notificationAddress.GetResourceIncludeList(),
            };

            return Ok(response);
        }
    }
}
