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
    /// Synchronizes the changes in the contact details for persons.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. If successful, returns a status 200 OK.
    /// </returns>
    /// <response code="200">Returns a <see cref="PersonContactPreferencesSnapshot"/> if the synchronization is successful.</response>
    /// <response code="400">Returns a bad request if the model state is invalid.</response>
    /// <response code="500">Returns a problem detail if an unexpected error occurs.</response>
    [HttpPost("syncchanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult SyncChanges()
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
