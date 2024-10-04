using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Integrations.Services;
using Altinn.Profile.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller to retrieve user notification preferences.
/// </summary>
[ApiController]
[Route("profile/api/v1/user-notification")]
[Consumes("application/json")]
[Produces("application/json")]
public class UserNotificationsController : ControllerBase
{
    private readonly IRegisterService _registerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotificationsController"/> class.
    /// </summary>
    /// <param name="registerService">The register service.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="registerService"/> is null.</exception>
    public UserNotificationsController(IRegisterService registerService)
    {
        _registerService = registerService ?? throw new ArgumentNullException(nameof(registerService));
    }

    /// <summary>
    /// Retrieves notification preferences for users based on their national identity numbers.
    /// </summary>
    /// <param name="request">A collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation, containing a response with user notification preferences.</returns>
    [HttpPost("GetNotificationPreferences")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserNotificationPreferencesResponse>> GetNotificationPreferences([FromBody] UserContactPointLookup request)
    {
        if (request?.NationalIdentityNumbers?.Count == 0)
        {
            return BadRequest("No national identity numbers provided.");
        }

        var validSSNs = request.NationalIdentityNumbers.Where(e => e.IsValidSocialSecurityNumber()).ToList();
        var invalidSSNs = request.NationalIdentityNumbers.Except(validSSNs).ToList();

        var notificationPreferences = await _registerService.GetUserContactInfoAsync(validSSNs);
        var matches = notificationPreferences.Select(np => new UserNotificationPreferences
        {
            NationalIdentityNumber = np.NationalIdentityNumber,
            Reservation = np.IsReserved,
            EmailAddress = np.EmailAddress,
            LanguageCode = np.LanguageCode,
            MobilePhoneNumber = np.MobilePhoneNumber,
        }).ToList();

        var noMatches = invalidSSNs.Select(invalid => new UserNotificationPreferences
        {
            NationalIdentityNumber = invalid
        }).ToList();

        noMatches.AddRange(validSSNs.Except(matches.Select(m => m.NationalIdentityNumber)).Select(item => new UserNotificationPreferences
        {
            NationalIdentityNumber = item
        }));

        var response = new UserNotificationPreferencesResponse(matches, noMatches);
        return Ok(response);
    }
}
