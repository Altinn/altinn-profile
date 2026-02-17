using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for logic concerning verification of notification addresses
    /// </summary>
    [Authorize]
    [Route("profile/api/v1/users/current/verification/")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AddressVerificationController : ControllerBase
    {
        private readonly IAddressVerificationService _addressVerificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressVerificationController"/> class.
        /// </summary>
        public AddressVerificationController(IAddressVerificationService addressVerificationService)
        {
            _addressVerificationService = addressVerificationService;
        }

        /// <summary>
        /// Get all verified addresses for the current user
        /// </summary>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [HttpGet("verified-addresses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<VerifiedAddressResponse>>> Get(CancellationToken cancellationToken)
        {
            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var verifiedAddresses = await _addressVerificationService.GetVerifiedAddressesAsync(userId, cancellationToken);
            var response = verifiedAddresses.Select(va => new VerifiedAddressResponse { Type = va.AddressType, Value = va.Address });

            return Ok(response);
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="request">The api request containing the aadress and code to verify</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<NotificationSettingsResponse>> Verify([FromBody]AddressVerificationRequest request, CancellationToken cancellationToken)
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

            var verified = await _addressVerificationService.SubmitVerificationCodeAsync(userId, request.Value, request.Type, request.VerificationCode, cancellationToken);

            if (!verified)
            {
                return UnprocessableEntity(new { Message = "Verification code is invalid or has expired." });
            }

            return Ok(new AddressVerificationResponse { Success = true });
        }
    }
}
