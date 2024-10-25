using System;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller responsible for managing changes in contact preferences for one or more persons.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
[Route("profile/api/v1/trigger")]
public class TriggerController : ControllerBase
{
    private readonly IPersonService _personService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TriggerController"/> class.
    /// </summary>
    /// <param name="personService">The service for retrieving the contact details.</param>
    public TriggerController(IPersonService personService)
    {
        _personService = personService;
    }

    /// <summary>
    /// Synchronizes the changes in the contact details for persons.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. If successful, returns a status 200 OK.
    /// </returns>
    /// <response code="200">Starting the synchronisation work was successfull.</response>
    /// <response code="500">Returns a problem detail if an unexpected error occurs.</response>
    [HttpGet("syncpersonchanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SyncChanges()
    {
        await _personService.SyncPersonContactPreferencesAsync();

        return Ok();
    }
}
