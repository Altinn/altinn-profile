using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Authorization;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Core.User;
using Altinn.Profile.Mappers;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organization notification addresses from the external registry.
    /// Used by the Support Dashboard to retrieve notification addresses registered in the business registry.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </remarks>
    [Authorize(Policy = AuthConstants.SupportDashboardAccess)]
    [Route("profile/api/v1/dashboard")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DashboardController(IOrganizationNotificationAddressesService notificationAddressService) : ControllerBase
    {
        private readonly IOrganizationNotificationAddressesService _notificationAddressService = notificationAddressService;

        /// <summary>
        /// Endpoint that can retrieve a list of all Notification Addresses for the given organization
        /// </summary>
        /// <returns>Returns the notification addresses for the provided organization</returns>                
        [HttpGet("organizations/{organizationNumber}/notificationaddresses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardNotificationAddressResponse>>> GetAllNotificationAddressesForAnOrg([FromRoute] string organizationNumber, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddresses([organizationNumber], cancellationToken, true);

            var orgCount = organizations.Count();

            if (orgCount == 0)
            {
                return NotFound();
            }
            else if (orgCount > 1)
            {
                throw new InvalidOperationException("Indecisive organization result");
            }

            var organization = organizations.First();
            var notificationAddresses = organization.NotificationAddresses;

            if (notificationAddresses == null)
            {
                return NotFound();
            }

            var addresses = FilterAndMapAddresses(organizations);

            return Ok(addresses);
        }

        /// <summary>
        /// Endpoint that can retrieve a list of all Notification Addresses for the given email address
        /// </summary>
        /// <returns>Returns the notification addresses for the provided email address</returns>                
        [HttpGet("organizations/notificationaddresses/email/{emailAddress}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardNotificationAddressResponse>>> GetNotificationAddressesByEmailAddress([FromRoute] string emailAddress, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddressesByEmailAddress(emailAddress, cancellationToken);

            var orgCount = organizations.Count();

            if (orgCount == 0)
            {
                return NotFound();
            }            

            var addresses = FilterAndMapAddresses(organizations);

            return Ok(addresses);
        }

        private static List<DashboardNotificationAddressResponse> FilterAndMapAddresses(IEnumerable<Organization> organizations)
        {
            var allAddresses = new List<DashboardNotificationAddressResponse>();

            foreach (var organization in organizations)
            {
                if (organization.NotificationAddresses == null)
                {
                    continue;
                }

                var addresses = organization.NotificationAddresses
                    .Where(n => n.IsSoftDeleted != true && n.HasRegistryAccepted != false)
                    .Select(n => OrganizationResponseMapper.ToDashboardNotificationAddressResponse(
                        n,
                        requestedOrgNumber: organization.OrganizationNumber,
                        sourceOrgNumber: organization.AddressOrigin))
                    .ToList();

                allAddresses.AddRange(addresses);
            }

            return allAddresses;
        }
    }

    /// <summary>
    /// Controller for user contact information registered for organizations.
    /// Used by the Support Dashboard to retrieve personal contact details that users have registered for acting on behalf of organizations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DashboardUserContactInformationController"/> class.
    /// </remarks>
    [Authorize(Policy = AuthConstants.SupportDashboardAccess)]
    [Route("profile/api/v1/dashboard")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DashboardUserContactInformationController(
        IRegisterClient registerClient,
        IProfessionalNotificationsRepository professionalNotificationsRepository,
        IUserProfileService userProfileService,
        ILogger<DashboardUserContactInformationController> logger) : ControllerBase
    {
        private readonly IRegisterClient _registerClient = registerClient;
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly ILogger<DashboardUserContactInformationController> _logger = logger;

        /// <summary>
        /// Sanitizes user-supplied strings before logging to prevent log forging attacks.
        /// Removes newline characters that could be used to inject fake log entries.
        /// </summary>
        private static string SanitizeForLog(string value) =>
            value?.Replace("\r", string.Empty).Replace("\n", string.Empty) ?? string.Empty;

        /// <summary>
        /// Endpoint that can retrieve a list of all user contact information for the given organization.
        /// Returns the contact details that users have registered for acting on behalf of this organization.
        /// </summary>
        /// <param name="organizationNumber">The organization number to retrieve contact information for</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Returns the user contact information for the provided organization</returns>
        /// <response code="200">Successfully retrieved user contact information. Returns an array of contacts (may be empty if organization exists but has no user contact info).</response>
        /// <response code="403">Caller does not have the required Dashboard Maskinporten scope (altinn:profile.support.admin).</response>
        /// <response code="404">Organization number not found in the registry.</response>
        [HttpGet("organizations/{organizationNumber}/contactinformation")]
        [ProducesResponseType(typeof(List<DashboardUserContactInformationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardUserContactInformationResponse>>> GetContactInformationByOrgNumber(
            [FromRoute] string organizationNumber,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Step 1: Translate orgNumber to partyUuid
            var parties = await _registerClient.GetPartyUuids(new[] { organizationNumber }, cancellationToken);

            if (parties == null || parties.Count == 0)
            {
                return NotFound();
            }

            if (parties.Count > 1)
            {
                _logger.LogWarning("Multiple parties found for organization number {OrganizationNumber}. Expected exactly one.", SanitizeForLog(organizationNumber));
                throw new InvalidOperationException("Multiple parties found for organization number");
            }

            var partyUuid = parties[0].PartyUuid;

            // Step 2: Get all user contact info for this party
            var contactInfos = await _professionalNotificationsRepository
                .GetAllNotificationAddressesForPartyAsync(partyUuid, cancellationToken) ?? [];

            // Step 3: Map to response - get user profiles and extract SSN/name
            var responses = new List<DashboardUserContactInformationResponse>();

            foreach (var contactInfo in contactInfos)
            {
                // Note: IUserProfileService.GetUser does not support cancellation token at this time
                var userProfileResult = await _userProfileService.GetUser(contactInfo.UserId);

                userProfileResult.Match(
                    profile =>
                    {
                        // Skip if Party data is missing (consistent with FilterAndMapAddresses pattern)
                        if (profile.Party == null)
                        {
                            _logger.LogWarning("User profile for UserId {UserId} in organization {OrganizationNumber} has no Party data. Skipping user.", contactInfo.UserId, SanitizeForLog(organizationNumber));
                            return;
                        }

                        responses.Add(new DashboardUserContactInformationResponse
                        {
                            NationalIdentityNumber = profile.Party.SSN ?? string.Empty,
                            Name = profile.Party.Name ?? string.Empty,
                            Email = contactInfo.EmailAddress,
                            Phone = contactInfo.PhoneNumber,
                            LastChanged = contactInfo.LastChanged
                        });
                    },
                    _ =>
                    {
                        _logger.LogWarning("Failed to retrieve user profile for UserId {UserId} in organization {OrganizationNumber}. Skipping user.", contactInfo.UserId, SanitizeForLog(organizationNumber));
                    });
            }

            // Return 200 OK even if empty list (matches acceptance criteria)
            return Ok(responses);
        }
    }
}
