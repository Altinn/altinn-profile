using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        /// Verify an address for the current user by providing the verification code sent to the address.
        /// </summary>
        /// <param name="request">The api request containing the aadress and code to verify</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        /// <remarks>This is rate limited</remarks>
        [HttpPost("verify")]
        [EnableRateLimiting("verify-address")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> Verify([FromBody]AddressVerificationRequest request, CancellationToken cancellationToken)
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
    }
}
