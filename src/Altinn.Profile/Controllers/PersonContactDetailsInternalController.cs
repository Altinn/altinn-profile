﻿using System;
using System.Threading.Tasks;

using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller to retrieve the contact details for one or more persons.
/// This controller is intended for internal consumption (e.g., Authorization) requiring neither authenticated user token nor access token authorization.
/// </summary>
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("profile/api/v1/internal/person/contact/details")]
public class PersonContactDetailsInternalController : ControllerBase
{
    private readonly ILogger<PersonContactDetailsInternalController> _logger;
    private readonly IPersonContactDetailsRetriever _contactDetailsRetriever;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonContactDetailsInternalController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="contactDetailsRetriever">The use case for retrieving the contact details.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contactDetailsRetriever"/> is null.</exception>
    public PersonContactDetailsInternalController(ILogger<PersonContactDetailsInternalController> logger, IPersonContactDetailsRetriever contactDetailsRetriever)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contactDetailsRetriever = contactDetailsRetriever ?? throw new ArgumentNullException(nameof(contactDetailsRetriever));
    }

    /// <summary>
    /// Retrieves the contact details for persons based on their national identity numbers.
    /// </summary>
    /// <param name="request">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a response with persons' contact details.
    /// Returns a <see cref="PersonContactDetailsLookupResult"/> with status 200 OK if successful,
    /// 400 Bad Request if the request is invalid, or 404 Not Found if no contact details are found.
    /// </returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PersonContactDetailsLookupResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonContactDetailsLookupResult>> PostLookup([FromBody] PersonContactDetailsLookupCriteria request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request?.NationalIdentityNumbers == null || request.NationalIdentityNumbers.Count == 0)
        {
            return BadRequest("National identity numbers cannot be null or empty.");
        }

        try
        {
            var result = await _contactDetailsRetriever.RetrieveAsync(request);

            return result.Match<ActionResult<PersonContactDetailsLookupResult>>(
                success =>
                {
                    return success?.MatchedPersonContactDetails?.Count > 0 ? Ok(success) : NotFound();
                },
                noMatch => NotFound());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details.");

            return Problem("An unexpected error occurred.");
        }
    }
}