using System;
using System.Threading.Tasks;

using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IContactDetailsRetriever _contactDetailsRetriever;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactDetailsController"/> class.
    /// </summary>
    /// <param name="contactDetailsRetriever">The use case for retrieving the contact details.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contactDetailsRetriever"/> is null.</exception>
    public ContactDetailsController(IContactDetailsRetriever contactDetailsRetriever)
    {
        _contactDetailsRetriever = contactDetailsRetriever ?? throw new ArgumentNullException(nameof(contactDetailsRetriever));
    }

    /// <summary>
    /// Retrieves the contact details for persons based on their national identity numbers.
    /// </summary>
    /// <param name="request">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a response with persons' contact details.
    /// Returns a <see cref="ContactDetailsLookupResult"/> with status 200 OK if successful.
    /// </returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ContactDetailsLookupResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ContactDetailsLookupResult>> PostLookup([FromBody] UserContactPointLookup request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (request.NationalIdentityNumbers == null)
            {
                return BadRequest();
            }

            if (request.NationalIdentityNumbers.Count == 0)
            {
                return BadRequest();
            }

            var result = await _contactDetailsRetriever.RetrieveAsync(request);

            return result.Match<ActionResult<ContactDetailsLookupResult>>(
                success =>
                {
                    if (success.MatchedContactDetails.Count != 0)
                    {
                        return Ok(success);
                    }

                    return NotFound();
                },
                noMatch => NotFound());
        }
        catch
        {
            return Problem("An unexpected error occurred.");
        }
    }
}
