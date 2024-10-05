using System;
using System.Threading.Tasks;

using Altinn.Profile.Models;
using Altinn.Profile.UseCases;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Controller to retrieve users contact details for internal consumption (e.g. Authorization) requiring neither authenticated user token nor access token authorization.
/// </summary>
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("profile/api/v1/contact/details/internal")]
public class UserContactDetailsInternalController : ControllerBase
{
    private readonly IUserContactDetailsRetriever _contactDetailsRetriever;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactDetailsInternalController"/> class.
    /// </summary>
    /// <param name="contactDetailsRetriever">The use case for retrieving user contact details.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contactDetailsRetriever"/> is null.</exception>
    public UserContactDetailsInternalController(IUserContactDetailsRetriever contactDetailsRetriever)
    {
        _contactDetailsRetriever = contactDetailsRetriever ?? throw new ArgumentNullException(nameof(contactDetailsRetriever));
    }

    /// <summary>
    /// Retrieves the contact details for users based on their national identity numbers.
    /// </summary>
    /// <param name="request">A collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation, containing a response with users' contact details.</returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserContactDetailsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserContactDetailsResult>> PostLookup([FromBody] UserContactPointLookup request)
    {
        var result = await _contactDetailsRetriever.RetrieveAsync(request);
        return result.Match<ActionResult<UserContactDetailsResult>>(
            success => Ok(success),
            failure => Problem("Failed to retrieve contact details. Please check the provided national identity numbers and try again."));
    }
}
