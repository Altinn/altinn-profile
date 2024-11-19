using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;

namespace Altinn.Profile.Telemetry;

/// <summary>
/// Helper class for telemetry configuration.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class TelemetryHelpers
{
    private readonly static string AuthenticationLevel = "urn:altinn:authlevel";
    private readonly static string UserId = "urn:altinn:userid";
    private readonly static string PartyID = "urn:altinn:partyid";
    private readonly static string OrgNumber = "urn:altinn:orgNumber";

    /// <summary>
    /// Checks if the given URL should be excluded from telemetry.
    /// </summary>
    /// <param name="url">The uri to check for excluded paths</param>
    /// <returns>true if the path should be excluded, otherwise false</returns>
    public static bool ShouldExclude(Uri url)
        => ShouldExclude(url.LocalPath.AsSpan());

    /// <summary>
    /// Checks if the given path should be excluded from telemetry.
    /// </summary>
    /// <param name="localPath">The path string to check for excluded paths</param>
    /// <returns>true if the path should be excluded, otherwise false</returns>
    public static bool ShouldExclude(PathString localPath)
        => ShouldExclude(localPath.HasValue ? localPath.Value.AsSpan() : []);

    /// <summary>
    /// Transfer some key values from the request to the activity.
    /// </summary>
    /// <param name="activity">The activity being registered</param>
    /// <param name="request">The triggering request</param>
    internal static void EnrichFromRequest(Activity activity, HttpRequest request)
    {
        if (!request.Path.ToString().Contains("profile/api/"))
        {
            return;
        }

        ClaimsPrincipal user = request.HttpContext.User;

        bool hasAuthLevel = false, hasPartyId = false, hasUserId = false, hasOrgNumber = false;
        foreach (var claim in user.Claims)
        {
            if (string.Equals(claim.Type, AuthenticationLevel, StringComparison.Ordinal))
            {
                if (!hasAuthLevel && int.TryParse(claim.Value, out var authLevel))
                {
                    hasAuthLevel = true;
                    activity.AddTag("altinn.auth_level", authLevel.ToString());
                }

                continue;
            }

            if (string.Equals(claim.Type, PartyID, StringComparison.Ordinal))
            {
                if (!hasPartyId && int.TryParse(claim.Value, out var partyId))
                {
                    hasPartyId = true;
                    activity.AddTag("altinn.party_id", partyId.ToString());
                }

                continue;
            }

            if (string.Equals(claim.Type, UserId, StringComparison.Ordinal))
            {
                if (!hasUserId && int.TryParse(claim.Value, out var userId))
                {
                    hasUserId = true;
                    activity.AddTag("altinn.user_id", userId.ToString());
                }

                continue;
            }

            if (string.Equals(claim.Type, OrgNumber, StringComparison.Ordinal))
            {
                if (!hasOrgNumber && int.TryParse(claim.Value, out var orgNumber))
                {
                    hasOrgNumber = true;
                    activity.AddTag("altinn.org_number", orgNumber.ToString());
                }

                continue;
            }
        }
    }

    private static bool ShouldExclude(ReadOnlySpan<char> localPath)
    {
        while (localPath.Length > 0 && localPath[^1] == '/')
        {
            localPath = localPath[..^1];
        }

        if (localPath.EndsWith("/health", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
