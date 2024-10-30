using System.Threading.Tasks;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller responsible for managing changes in contact preferences for one or more persons.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TriggerController"/> class.
/// </remarks>
/// <param name="contactRegisterUpdateJob">The service for retrieving the contact details.</param>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
[Route("profile/api/v1/trigger")]
public class TriggerController(IContactRegisterUpdateJob contactRegisterUpdateJob) : ControllerBase
{
    private readonly IContactRegisterUpdateJob _contactRegisterUpdateJob = contactRegisterUpdateJob;

    /// <summary>
    /// Synchronizes the changes in the contact details for persons.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. If successful, returns a status 200 OK.
    /// </returns>
    /// <response code="200">Starting the synchronisation work was successfull.</response>
    [HttpGet("syncpersonchanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SyncChanges()
    {
        await _contactRegisterUpdateJob.SyncContactInformationAsync();

        return Ok();
    }
}
