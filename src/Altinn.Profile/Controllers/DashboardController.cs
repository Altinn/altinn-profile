using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Mappers;
using Altinn.Profile.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers
{
    /// <summary>
    /// Controller for organization notification addresses from the external registry.
    /// Used by the Support Dashboard to retrieve notification addresses registered in the business registry.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </remarks>
    [ApiController]
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

        /// <summary>
        /// Endpoint that can retrieve a list of all Notification Addresses for the given phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to retrieve notification addresses for</param>
        /// <param name="countryCode">The country code for the phone number (default: +47)</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Returns the notification addresses for the provided phone number</returns> 
        [HttpGet("organizations/notificationaddresses/phonenumber/{phoneNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardNotificationAddressResponse>>> GetNotificationAddressesByPhoneNumber(
            [FromRoute(Name = "phoneNumber"), Required, RegularExpression(@"^\d{5,8}$", ErrorMessage = "The phone number is not valid. It must contain only digits and be between 5 and 8 digits long.")] string phoneNumber,
            [FromQuery(Name = "countrycode"), RegularExpression(@"^(?:\+|00)\d{1,3}$", ErrorMessage = "The country code is not valid. It must be between 1 to 3 digits, prefixed with '+' or '00'.")] string countryCode = "+47",
            CancellationToken cancellationToken = default)
        {            
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var organizations = await _notificationAddressService.GetOrganizationNotificationAddressesByPhoneNumber(phoneNumber, countryCode, cancellationToken);

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
    [ApiController]
    [Authorize(Policy = AuthConstants.SupportDashboardAccess)]
    [Route("profile/api/v1/dashboard")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DashboardUserContactInformationController(
        IProfessionalNotificationsService professionalNotificationsService) : ControllerBase
    {
        private readonly IProfessionalNotificationsService _professionalNotificationsService = professionalNotificationsService;

        /// <summary>
        /// Endpoint that can retrieve a list of all user contact information for the given organization.
        /// Returns the contact details that users have registered for acting on behalf of this organization.
        /// </summary>
        /// <param name="organizationNumber">The organization number to retrieve contact information for</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Returns the user contact information for the provided organization</returns>
        /// <response code="200">Successfully retrieved user contact information. Returns an array of contacts (may be empty if organization exists but has no user contact info).</response>
        /// <response code="400">Invalid request parameters (model validation failed).</response>
        /// <response code="403">Caller does not have the required Dashboard Maskinporten scope (altinn:profile.support.admin).</response>
        /// <response code="404">Organization number not found in the registry.</response>
        [HttpGet("organizations/{organizationNumber}/contactinformation")]
        [ProducesResponseType(typeof(List<DashboardUserContactInformationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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

            var contactInfos = await _professionalNotificationsService
                .GetContactInformationByOrganizationNumberAsync(organizationNumber, cancellationToken);

            if (contactInfos.Count == 0)
            {
                return Ok(new List<DashboardUserContactInformationResponse>());
            }

            var responses = MapContactInfosToResponses(contactInfos);

            return Ok(responses);
        }

        /// <summary>
        /// Endpoint that can retrieve a list of all user contact information for the given email address.
        /// Returns the contact details that users have registered with the specified email address.
        /// </summary>
        /// <param name="emailAddress">The email address to retrieve contact information for</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Returns the user contact information for the provided email address</returns> 
        /// <response code="200">Successfully retrieved user contact information. Returns an array of contacts for the specified email address (empty array if no contacts found)</response>
        /// <response code="400">Invalid request parameters (model validation failed).</response>
        /// <response code="403">Caller does not have the required Dashboard Maskinporten scope (altinn:profile.support.admin).</response>
        [HttpGet("organizations/contactinformation/email/{emailAddress}")]
        [ProducesResponseType(typeof(List<DashboardUserContactInformationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardUserContactInformationResponse>>> GetContactInformationByEmailAddress(
            [FromRoute] string emailAddress,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var contactInfosByEmail = await _professionalNotificationsService
                .GetContactInformationByEmailAddressAsync(emailAddress, cancellationToken);

            if (contactInfosByEmail.Count == 0)
            {
                return Ok(new List<DashboardUserContactInformationResponse>());
            }

            var responses = MapContactInfosToResponses(contactInfosByEmail);

            return Ok(responses);
        }

        /// <summary>
        /// Endpoint that can retrieve a list of all user contact information for the given phone number.
        /// Returns the contact details that users have registered with the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to retrieve contact information for</param>
        /// <param name="countryCode">The country code for the phone number</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Returns the user contact information for the provided phone number</returns> 
        /// <response code="200">Successfully retrieved user contact information. Returns an array of contacts for the specified phone number (empty array if no contacts found)</response>
        /// <response code="400">Invalid request parameters (model validation failed).</response>
        /// <response code="403">Caller does not have the required Dashboard Maskinporten scope (altinn:profile.support.admin).</response>
        [HttpGet("organizations/contactinformation/phonenumber/{phoneNumber}")]
        [ProducesResponseType(typeof(List<DashboardUserContactInformationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DashboardUserContactInformationResponse>>> GetContactInformationByPhoneNumber(
             [FromRoute(Name = "phoneNumber"), Required, RegularExpression(@"^\d{5,8}$", ErrorMessage = "The phone number is not valid. It must contain only digits and be between 5 and 8 digits long.")] string phoneNumber,
             [FromQuery(Name = "countrycode"), RegularExpression(@"^(?:\+|00)\d{1,3}$", ErrorMessage = "The country code is not valid. It must be between 1 to 3 digits, prefixed with '+' or '00'.")] string countryCode,
             CancellationToken cancellationToken = default)          
        {
             if (!ModelState.IsValid)
             {
                return ValidationProblem(ModelState);
             }

             var contactInfosByPhone = await _professionalNotificationsService.GetContactInformationByPhoneNumberAsync(phoneNumber, countryCode, cancellationToken);

             if (contactInfosByPhone.Count == 0)
             {
                return Ok(new List<DashboardUserContactInformationResponse>());
             }

             var responses = MapContactInfosToResponses(contactInfosByPhone);

             return Ok(responses);
        }

        private static List<DashboardUserContactInformationResponse> MapContactInfosToResponses(IEnumerable<UserPartyContactInfoWithIdentity> contactInfos)
        {
            return [.. contactInfos.Select(c => new DashboardUserContactInformationResponse
            {
                NationalIdentityNumber = c.NationalIdentityNumber,
                Name = c.Name,
                Email = c.EmailAddress,
                Phone = c.PhoneNumber,
                OrganizationNumber = c.OrganizationNumber,
                LastChanged = c.LastChanged
            })];
        }
    }
}
