using System;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller responsible for managing changes in contact preferences for one or more persons.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TriggerController"/> class.
/// </remarks>
/// <param name="contactRegisterUpdateJob">The service for retrieving the contact details.</param>
/// <param name="orgUpdateJob">The service for retrieving the notificationaddresses for organizations.</param>
/// <param name="logger">A logger to log detailed information.</param>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
[Route("profile/api/v1/trigger/syncpersonchanges")]
public class TriggerController(IContactRegisterUpdateJob contactRegisterUpdateJob, IOrganizationNotificationAddressSyncJob orgUpdateJob, ILogger<TriggerController> logger) : ControllerBase
{
    private readonly ILogger<TriggerController> _logger = logger;
    private readonly IContactRegisterUpdateJob _contactRegisterUpdateJob = contactRegisterUpdateJob;
    private readonly IOrganizationNotificationAddressSyncJob _orgUpdateJob = orgUpdateJob;

    /// <summary>
    /// Synchronizes the changes in the contact details for persons.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. If successful, returns a status 200 OK.
    /// </returns>
    /// <response code="200">Starting the synchronization work was successful.</response>
    /// <response code="500">An error occurred while starting the synchronization.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SyncChanges()
    {
        try
        {
            await _contactRegisterUpdateJob.SyncContactInformationAsync();

            return Ok("Synchronization has completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the background synchronization.");

            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while synchronizing the changes.");
        }
    }
}

/// <summary>
/// Controller responsible for managing changes in contact preferences for one or more persons.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TriggerController"/> class.
/// </remarks>
/// <param name="orgUpdateJob">The service for retrieving the notificationaddresses for organizations.</param>
/// <param name="logger">A logger to log detailed information.</param>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
[Route("profile/api/v1/trigger/syncorgchanges")]
public class TriggerOrgSyncController(IOrganizationNotificationAddressSyncJob orgUpdateJob, ILogger<TriggerOrgSyncController> logger) : ControllerBase
{
    private readonly ILogger<TriggerOrgSyncController> _logger = logger;
    private readonly IOrganizationNotificationAddressSyncJob _orgUpdateJob = orgUpdateJob;

    /// <summary>
    /// Synchronizes the notification addresses for organizations.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. If successful, returns a status 200 OK.
    /// </returns>
    /// <response code="200">Starting the synchronization work was successful.</response>
    /// <response code="500">An error occurred while starting the synchronization.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SyncOrgChanges()
    {
        try
        {
            await _orgUpdateJob.SyncNotificationAddressesAsync();

            return Ok("Synchronization has completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the background synchronization.");

            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while synchronizing the changes.");
        }
    }
}
