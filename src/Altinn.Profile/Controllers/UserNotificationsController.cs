using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    private readonly INationalIdentityNumberChecker _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotificationsController"/> class.
    /// </summary>
    /// <param name="registerService">The register service.</param>
    /// <param name="validator">The social security number validator</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="registerService"/> is null.</exception>
    public UserNotificationsController(IRegisterService registerService, INationalIdentityNumberChecker validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
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
    [ProducesResponseType(typeof(UserContactDetailsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserContactDetailsResult>> GetNotificationPreferences([FromBody] UserContactPointLookup request)
    {
        if (request?.NationalIdentityNumbers?.Count == 0)
        {
            return BadRequest();
        }

        var notificationPreferences = await _registerService.GetUserContactAsync(request.NationalIdentityNumbers).ConfigureAwait(false);

        var matches = notificationPreferences.MatchedUserContact.Select(e => new UserContactDetails
        {
            Reservation = e.IsReserved,
            EmailAddress = e.EmailAddress,
            LanguageCode = e.LanguageCode,
            MobilePhoneNumber = e.MobilePhoneNumber,
            NationalIdentityNumber = e.NationalIdentityNumber,
        }).ToImmutableList();

        // Create a list for no matches
        var noMatches =  notificationPreferences.UnmatchedUserContact.Select(e => new UserContactDetails
        {
            NationalIdentityNumber = e.NationalIdentityNumber,
        }).ToImmutableList();

        var response = new UserContactDetailsResult(matches, [.. noMatches]);
        return Ok(response);
    }
}
