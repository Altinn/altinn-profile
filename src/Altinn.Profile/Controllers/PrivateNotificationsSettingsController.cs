using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactInfo;
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
    [Route("profile/api/v1/users/current/notificationsettings/private")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class PrivateNotificationsSettingsController : ControllerBase
    {
        private readonly IUserContactInfoService _userContactInfoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsSettingsController"/> class.
        /// </summary>
        public PrivateNotificationsSettingsController(IUserContactInfoService userContactInfoService)
        {
            _userContactInfoService = userContactInfoService;
        }
   
        /// <summary>
        /// Add or update the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="request"> The request containing the notification address details</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [HttpPut("phonenumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> Put([FromBody][Required] PrivateNotificationSettingsRequest request, CancellationToken cancellationToken)
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

            //// TODO: Validate that the user is self-identified

            var isVerifiedOrNull = await _userContactInfoService.IsAddressVerifiedOrNull(userId, request.Value, cancellationToken);
            if (!isVerifiedOrNull)
            {
                return UnprocessableEntity("Provided email address or phone number is not verified.");
            }

            var response = await _userContactInfoService.UpdatePhoneNumber(userId, request.Value, cancellationToken);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(new PrivateNotificationSettingsRequest { Value = response });
        }
    }
}
