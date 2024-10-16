using System;

using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller responsible for managing changes in contact preferences for one or more persons.
/// </summary>
[Authorize]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("profile/api/v1/person/changes")]
public class PersonContactChangesController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonContactChangesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactChangesController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="personService">The service for retrieving the contact details.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="logger"/> or <paramref name="personService"/> is null.
    /// </exception>
    public PersonContactChangesController(ILogger<PersonContactChangesController> logger, IPersonService personService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
    }

    /// <summary>
    /// Retrieves the contact details for persons based on their national identity numbers.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a response with persons' contact details.
    /// Returns a <see cref="PersonContactPreferencesSnapshot"/> with status 200 OK if successful.
    /// </returns>
    [HttpPost("SyncContentChanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SyncContentChanges()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _personService.SyncPersonContactPreferencesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details.");

            return Problem("An unexpected error occurred.");
        }
    }
}
