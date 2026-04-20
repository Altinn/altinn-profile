using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Utils;
using Altinn.Profile.Models;
using Altinn.Profile.Models.ProfessionalNotificationSettings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organizing the notification addresses a user has registered for parties
    /// </summary>
    [Authorize]
    [Route("profile/api/v1/users/current/notificationsettings/parties")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ProfessionalNotificationSettingsController : ControllerBase
    {
        private readonly IProfessionalNotificationsService _professionalNotificationsService;
        private const string _partyUuidEmptyErrorMessage = "Party UUID cannot be empty.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfessionalNotificationSettingsController"/> class.
        /// </summary>
        public ProfessionalNotificationSettingsController(IProfessionalNotificationsService professionalNotificationsService)
        {
            _professionalNotificationsService = professionalNotificationsService;
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being set</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpGet("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationSettingsResponse>> Get([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
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
                return BadRequest(_partyUuidEmptyErrorMessage);
            }

            var notificationSettings = await _professionalNotificationsService.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);

            if (notificationSettings == null)
            {
                return NotFound("Notification addresses not found for the specified user and party.");
            }

            var response = MapResponse(notificationSettings);

            return Ok(response);
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for all parties
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IReadOnlyList<NotificationSettingsResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var validationResult = ClaimsHelper.TryGetUserIdFromClaims(Request.HttpContext, out int userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            var notificationSettings = await _professionalNotificationsService.GetAllNotificationAddressesAsync(userId, cancellationToken);

            var response = notificationSettings.Select(MapResponse).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Add or update the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being set</param>
        /// <param name="request"> The request containing the notification address details</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpPut("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> Put([FromRoute] Guid partyUuid, [FromBody][Required] NotificationSettingsRequest request, CancellationToken cancellationToken)
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
                return BadRequest(_partyUuidEmptyErrorMessage);
            }

            var userPartyContactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
                UserPartyContactInfoResources = request.ResourceIncludeList?
                    .Select(resource => ResourceIdFormatter.GetSanitizedResourceId(resource))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => new UserPartyContactInfoResource { ResourceId = s })
                    .ToList()
            };
            var added = await _professionalNotificationsService.AddOrUpdateNotificationAddressAsync(userPartyContactInfo, cancellationToken);

            if (added)
            {
                return CreatedAtAction(nameof(Get), new { partyUuid }, null);
            }

            return NoContent();
        }

        /// <summary>
        /// Add or update the notification addresses the current user has registered for a party. This endpoint allows partial updates, meaning that the user can choose to update only the email address,
        /// only the phone number, or only the resource include list without affecting the other fields.
        /// </summary>
        /// <remarks>
        /// Note that addresses are required to be verified. If the user attempts to add or update an address that has not been verified, the operation will be rejected with a 422 Unprocessable Entity response, and the notification address will not be added or updated.
        /// </remarks>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being set</param>
        /// <param name="request"> The request containing the notification address details</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpPatch("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> Patch([FromRoute] Guid partyUuid, [FromBody][Required] NotificationSettingsPatchRequest request, CancellationToken cancellationToken)
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
                return BadRequest(_partyUuidEmptyErrorMessage);
            }

            var userPartyContactInfo = new PatchUserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
            };

            if (request.ResourceIncludeList.HasValue)
            {
                var resources = request.ResourceIncludeList.Value;
                if (resources == null || resources.Count == 0)
                {
                    userPartyContactInfo.UserPartyContactInfoResources = new Optional<List<UserPartyContactInfoResource>>([]);
                }
                else
                {
                    userPartyContactInfo.UserPartyContactInfoResources = new Optional<List<UserPartyContactInfoResource>>(resources
                    .Select(resource => ResourceIdFormatter.GetSanitizedResourceId(resource))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => new UserPartyContactInfoResource { ResourceId = s })
                    .ToList());
                }
            }

            var isVerifiedOrNull = await _professionalNotificationsService.IsContactInfoVerifiedOrNullAsync(userPartyContactInfo, cancellationToken);
            if (!isVerifiedOrNull)
            {
                return UnprocessableEntity("Provided email address or phone number is not verified.");
            }

            var added = await _professionalNotificationsService.AddOrUpdateNotificationAddressAsync(userPartyContactInfo, cancellationToken);

            if (added)
            {
                return CreatedAtAction(nameof(Get), new { partyUuid }, null);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being deleted</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpDelete("{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationSettingsResponse>> Delete([FromRoute] Guid partyUuid, CancellationToken cancellationToken)
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
                return BadRequest(_partyUuidEmptyErrorMessage);
            }

            var notificationAddress = await _professionalNotificationsService.DeleteNotificationAddressAsync(userId, partyUuid, cancellationToken);

            if (notificationAddress == null)
            {
                return NotFound("Notification addresses not found for the specified user and party.");
            }

            return Ok();
        }

        private NotificationSettingsResponse MapResponse(ExtendedUserPartyContactInfo notificationAddress)
        {
            return new NotificationSettingsResponse
            {
                UserId = notificationAddress.UserId,
                PartyUuid = notificationAddress.PartyUuid,
                EmailAddress = notificationAddress.EmailAddress,
                PhoneNumber = notificationAddress.PhoneNumber,
                ResourceIncludeList = notificationAddress.GetResourceIncludeList(),
                NeedsConfirmation = notificationAddress.NeedsConfirmation,
                SmsVerificationStatus = notificationAddress.SmsVerificationStatus,
                EmailVerificationStatus = notificationAddress.EmailVerificationStatus
            };
        }
    }
}
