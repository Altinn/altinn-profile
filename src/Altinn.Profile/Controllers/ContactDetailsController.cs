﻿using System;
using System.Threading.Tasks;

using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller to retrieve the contact details for one or more persons.
/// </summary>
[Authorize]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("profile/api/v1/contact/details")]
public class ContactDetailsController : ControllerBase
{
    private readonly ILogger<ContactDetailsController> _logger;
    private readonly IContactDetailsRetriever _contactDetailsRetriever;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactDetailsController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="contactDetailsRetriever">The use case for retrieving the contact details.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="logger"/> or <paramref name="contactDetailsRetriever"/> is null.
    /// </exception>
    public ContactDetailsController(ILogger<ContactDetailsController> logger, IContactDetailsRetriever contactDetailsRetriever)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contactDetailsRetriever = contactDetailsRetriever ?? throw new ArgumentNullException(nameof(contactDetailsRetriever));
    }

    /// <summary>
    /// Retrieves the contact details for persons based on their national identity numbers.
    /// </summary>
    /// <param name="lookupCriteria">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a response with persons' contact details.
    /// Returns a <see cref="ContactDetailsLookupResult"/> with status 200 OK if successful.
    /// </returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ContactDetailsLookupResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContactDetailsLookupResult>> PostLookup([FromBody] UserContactPointLookup lookupCriteria)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (lookupCriteria?.NationalIdentityNumbers == null || lookupCriteria.NationalIdentityNumbers.Count == 0)
        {
            return BadRequest("National identity numbers cannot be null or empty.");
        }

        try
        {
            var lookupResult = await _contactDetailsRetriever.RetrieveAsync(lookupCriteria);

            return lookupResult.Match<ActionResult<ContactDetailsLookupResult>>(
                successResponse =>
                {
                    return successResponse?.MatchedContactDetails?.Count > 0 ? Ok(successResponse) : NotFound();
                },
                failedResponse => NotFound());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details.");

            return Problem("An unexpected error occurred.");
        }
    }
}
