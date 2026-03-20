using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core;
using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        private readonly int _verificationCodeCooldownPeriodInSeconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressVerificationController"/> class.
        /// </summary>
        public AddressVerificationController(IAddressVerificationService addressVerificationService, IOptions<AddressMaintenanceSettings> addressMaintenanceSettings)
        {
            _addressVerificationService = addressVerificationService;
            _verificationCodeCooldownPeriodInSeconds = addressMaintenanceSettings.Value.VerificationCodeResendCooldownSeconds;
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
        /// Verify an address for the current user by providing the verification code sent to the address.
        /// </summary>
        /// <param name="request">The code to verify, the address type, and the address value</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        /// <remarks>
        /// This endpoint is rate limited using a sliding window rate limiter to prevent abuse.
        /// Rate limit: 10 requests per 1 minute window.
        /// When the limit is exceeded, a 429 Too Many Requests response is returned.
        /// For more information about sliding window rate limiting, see:
        /// https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/#sliding-window-limit
        /// </remarks>
        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> Verify([FromBody][Required] AddressVerificationRequest request, CancellationToken cancellationToken)
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

            var verified = await _addressVerificationService.SubmitVerificationCodeAsync(userId, request.Value, (AddressType)request.Type, request.VerificationCode, cancellationToken);

            if (!verified)
            {
                return UnprocessableEntity(new ProblemDetails { Title = "Address could not be verified", Detail = "The given verification code does not validate for the given address." });
            }

            return NoContent();
        }

        /// <summary>
        /// Starts the verification process for the current user and the given address, by generating a code with a validity period and sending it to the address.
        /// This can also be used to resend a new code for an address that is already in the verification process, but not yet verified, by generating a new code and invalidating the previous one. 
        /// However, if a code has already been sent and is still within its cooldown period, a new code will not be generated, and the existing code's cooldown will be returned in the response to inform the user when they can attempt to resend.
        /// </summary>
        /// <param name="request">The address type and value to send code for</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        /// <response code="200">Indicates that the verification code was successfully generated and sent</response>
        /// <response code="400">Indicates that the request was malformed, e.g. missing required properties or invalid address format</response>
        /// <response code="403">Indicates that the user is not authenticated</response>
        /// <response code="422">Indicates that the address is already verified for the user, and thus a code cannot be sent</response>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> Send([FromBody][Required] AddressCodeResendRequest request, CancellationToken cancellationToken)
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

            var sendResult = await _addressVerificationService.SendVerificationCodeAsync(userId, request.Value, (AddressType)request.Type, cancellationToken);

            return sendResult.Status switch
            {
                SendVerificationStatus.Success when sendResult.NotificationSent => new OkObjectResult(new AddressVerificationResponse { CooldownSeconds = sendResult.Cooldown, NotificationSent = true }),
                SendVerificationStatus.Success => InternalServerError(new ProblemDetails { Title = "Verification code could not be sent", Detail = "The verification process was created, but notification delivery failed." }),
                SendVerificationStatus.AddressAlreadyVerified => UnprocessableEntity(new ProblemDetails { Title = "Verification code could not be sent", Detail = "The address is already verified for this user." }),
                SendVerificationStatus.CodeCooldown => new OkObjectResult(new AddressVerificationResponse { CooldownSeconds = sendResult.Cooldown, NotificationSent = false }),
                _ => InternalServerError(new ProblemDetails { Title = "Verification code could not be sent", Detail = "An unexpected error occurred." })
            };
        }

        /// <summary>
        /// Resets the verification process for the current user and the given address, by regenerating a code with a renewed validity period and sending it to the address.
        /// </summary>
        /// <param name="request">The address type and value to resend code for</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [HttpPost("resend")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> Resend([FromBody][Required] AddressCodeResendRequest request, CancellationToken cancellationToken)
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

            var result = await _addressVerificationService.ResendVerificationCodeAsync(userId, request.Value, (AddressType)request.Type, cancellationToken);

            return result switch
            {
                SendVerificationStatus.Success => NoContent(),
                SendVerificationStatus.CodeNotFound => UnprocessableEntity(new ProblemDetails { Title = "Verification code could not be resent", Detail = "The user has no active verification process for the given address." }),
                SendVerificationStatus.CodeCooldown => TooManyRequests(new ProblemDetails { Title = "Verification code could not be resent", Detail = $"Code resending attempts for an address are limited to 1 request per {_verificationCodeCooldownPeriodInSeconds} seconds. Please wait before requesting a new code." }),
                _ => InternalServerError(new ProblemDetails { Title = "Verification code could not be resent", Detail = "An unexpected error occurred." })
            };
        }

        private ObjectResult TooManyRequests(ProblemDetails problemDetails)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, problemDetails);
        }

        private ObjectResult InternalServerError(ProblemDetails problemDetails)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }
}
