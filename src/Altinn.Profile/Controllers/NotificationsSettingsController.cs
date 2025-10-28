using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Configuration;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        private readonly AltinnConfiguration _altinnConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsSettingsController"/> class.
        /// </summary>
        public NotificationsSettingsController(IProfessionalNotificationsService professionalNotificationsService, IOptions<AltinnConfiguration> altinnConfiguration)
        {
            _professionalNotificationsService = professionalNotificationsService;
            _altinnConfiguration = altinnConfiguration.Value;
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being set</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpGet("parties/{partyUuid:guid}")]
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
                return BadRequest("Party UUID cannot be empty.");
            }

            var (notificationSettings, profileSettings) = await _professionalNotificationsService.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);

            if (notificationSettings == null)
            {
                return NotFound("Notification addresses not found for the specified user and party.");
            }

            var response = MapResponse(notificationSettings, profileSettings);

            return Ok(response);
        }

        /// <summary>
        /// Get the notification addresses the current user has registered for all parties
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        [HttpGet("parties")]
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

            var result = await _professionalNotificationsService.GetAllNotificationAddressesAsync(userId, cancellationToken);

            var response = result.NotificationSettings.Select(n => MapResponse(n, result.ProfileSettings)).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Add or update the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being set</param>
        /// <param name="request"> The request containing the notification address details</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpPut("parties/{partyUuid:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> Put([FromRoute] Guid partyUuid, [FromBody] NotificationSettingsRequest request, CancellationToken cancellationToken)
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
        /// Delete the notification addresses the current user has registered for a party
        /// </summary>
        /// <param name="partyUuid">The UUID of the party for which the notification address is being deleted</param>
        /// <param name="cancellationToken"> Cancellation token for the operation</param>
        [Authorize(Policy = AuthConstants.UserPartyAccess)]
        [HttpDelete("parties/{partyUuid:guid}")]
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
                return BadRequest("Party UUID cannot be empty.");
            }

            var notificationAddress = await _professionalNotificationsService.DeleteNotificationAddressAsync(userId, partyUuid, cancellationToken);

            if (notificationAddress == null)
            {
                return NotFound("Notification addresses not found for the specified user and party.");
            }

            return Ok();
        }

        private NotificationSettingsResponse MapResponse(UserPartyContactInfo notificationAddress, ProfileSettings profileSettingPreference)
        {
            return new NotificationSettingsResponse
            {
                UserId = notificationAddress.UserId,
                PartyUuid = notificationAddress.PartyUuid,
                EmailAddress = notificationAddress.EmailAddress,
                PhoneNumber = notificationAddress.PhoneNumber,
                ResourceIncludeList = notificationAddress.GetResourceIncludeList(),
                NeedsConfirmation = NeedsConfirmation(notificationAddress, profileSettingPreference)
            };
        }

        private bool NeedsConfirmation(UserPartyContactInfo notificationAddress, ProfileSettings profileSettingPreference)
        {
            TimeSpan daysSinceIgnore = DateTime.Now - (profileSettingPreference.IgnoreUnitProfileDateTime ?? DateTime.MinValue);
            if (daysSinceIgnore.TotalDays <= _altinnConfiguration.IgnoreUnitProfileConfirmationDays)
            {
                return false;
            }

            var lastModified = notificationAddress.LastChanged;

            var daysSinceLastUserUnitProfileUpdate = (DateTime.Now - lastModified).TotalDays;
            if (daysSinceLastUserUnitProfileUpdate >= _altinnConfiguration.ValidationReminderDays)
            {
                return true;
            }

            return false;
        }
    }
}
