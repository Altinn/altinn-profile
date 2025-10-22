using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Configuration;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Unit.ContactPoints;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Controllers;

/// <summary>
/// This controller provides an internal endpoint for accessing user-registered notification addresses
/// for organizations. The notification addresses are typically registered by users in a professional context.
/// </summary>
/// <remarks>
/// The endpoints are intended for internal consumption (e.g. Notifications) and do not require authenticated
/// user token or access token authorization.
/// </remarks>
[Route("profile/api/v1/units/contactpoint")]
[ApiExplorerSettings(IgnoreApi = true)]
[Consumes("application/json")]
[Produces("application/json")]
public class UnitContactPointController : ControllerBase
{
    private readonly IUnitContactPointsService _contactPointsService;
    private readonly IOptionsMonitor<GeneralSettings> _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitContactPointController"/> class.
    /// </summary>
    /// <param name="contactPointsService">
    /// A service implementation of <see cref="IUnitContactPointsService"/> that handles business logic
    /// related to professional notification addresses.
    /// </param>
    /// <param name="settings">The general settings.</param>
    public UnitContactPointController(IUnitContactPointsService contactPointsService, IOptionsMonitor<GeneralSettings> settings)
    {
        _contactPointsService = contactPointsService;
        _settings = settings;
    }

    /// <summary>
    /// Endpoint for looking up user-registered notification addresses for the provided organizations and the
    /// given resource id.
    /// </summary>
    /// <param name="unitContactPointLookup">The search criteria.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// Returns a list of user-registered notification addresses for the provided units.
    /// </returns>
    [HttpPost("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<ActionResult<UnitContactPointsList>> PostLookup(
        [FromBody] UnitContactPointLookup unitContactPointLookup, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (_settings.CurrentValue.LookupUnitContactPointsAtSblBridge)
        {
            Result<UnitContactPointsList, bool> result =
            await _contactPointsService.GetUserRegisteredContactPoints(unitContactPointLookup);

            return result.Match<ActionResult<UnitContactPointsList>>(
                success => Ok(success),
                _ => Problem("Could not retrieve contact points"));
        }
        else
        {
            try
            {
                var resourceId = GetSanitizedResourceId(unitContactPointLookup.ResourceId);
                var organizationNumbers = unitContactPointLookup.OrganizationNumbers.Distinct().Select(o => o.Trim());
                var result = await _contactPointsService.GetUserRegisteredContactPoints([..organizationNumbers], resourceId, cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {
                return Problem($"Could not retrieve contact points");
            }
        }
    }

    /// <summary>
    /// Normalizes a resource identifier value by removing the leading
    /// 'urn:altinn:resource:' prefix if it is present.
    /// </summary>
    /// <param name="resourceId">
    /// The raw resource identifier (may be a plain slug like 'tax-report', or
    /// a full attribute value starting with 'urn:altinn:resource:').
    /// Can be <c>null</c> or whitespace.
    /// </param>
    /// <returns>
    /// The resource identifier without the 'urn:altinn:resource:' prefix, or
    /// <see cref="string.Empty"/> when the input is <c>null</c> or whitespace.
    /// </returns>
    private static string GetSanitizedResourceId(string resourceId)
    {
        var trimmedResourceId = resourceId?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedResourceId))
        {
            return string.Empty;
        }

        const string prefix = "urn:altinn:resource:";

        return trimmedResourceId.StartsWith(prefix, StringComparison.Ordinal) ? trimmedResourceId[prefix.Length..] : trimmedResourceId;
    }
}
