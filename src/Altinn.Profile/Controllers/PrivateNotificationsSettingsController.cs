using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organizing the notification addresses for self-identified users.
    /// </summary>
    [Authorize]
    [Route("profile/api/v1/users/current/notificationsettings/private")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class PrivateNotificationsSettingsController : ControllerBase
    {
        private readonly IUserContactInfoService _userContactInfoService;
        private readonly IAddressVerificationService _addressVerificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateNotificationsSettingsController"/> class.
        /// </summary>
        public PrivateNotificationsSettingsController(IUserContactInfoService userContactInfoService, IAddressVerificationService addressVerificationService)
        {
            _userContactInfoService = userContactInfoService;
            _addressVerificationService = addressVerificationService;
        }

        /// <summary>
        /// Add or update the telephone number for a self-identified user. The phone number must be verified before it can be added as a notification address. 
        /// If the user already has a phone number registered, it will be replaced with the new one.
        /// </summary>
        /// <param name="request"> The request containing the notification address details</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [HttpPut("phonenumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PrivateNotificationSettingsResponse>> Put([FromBody][Required] PrivateNotificationSettingsRequest request, CancellationToken cancellationToken)
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

            if (!IsSelfIdentifiedUser(Request.HttpContext))
            {
                return Forbid();
            }

            var isVerifiedOrNull = await _addressVerificationService.IsAddressVerifiedOrNull(userId, AddressType.Sms, request.Value, cancellationToken);
            if (!isVerifiedOrNull)
            {
                return UnprocessableEntity("Provided phone number is not verified.");
            }

            var response = await _userContactInfoService.UpdatePhoneNumber(userId, request.Value, cancellationToken);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(new PrivateNotificationSettingsResponse { Value = response.PhoneNumber });
        }

        private static bool IsSelfIdentifiedUser(HttpContext httpContext)
        {
            var authenticationMethod = ClaimsHelper.GetAuthenticateMethodAsString(httpContext);
            return authenticationMethod == "SelfIdentified" || authenticationMethod == "IdportenEpost";
        }
    }
}
