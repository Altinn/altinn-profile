#nullable enable

using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace Altinn.Profile.Authorization;

/// <summary>
/// Authorization handler to deny access to endpoint if the user was authenticated using a specific method
/// or if the user does not have an authentication method claim at all.
/// </summary>
public class DenyAuthenticationMethodHandler : AuthorizationHandler<DenyAuthenticationMethodRequirement>
{
    private const string AuthenticationMethodClaimName = "urn:altinn:authenticatemethod";

    /// <summary>
    /// This method authorizes access based on context and requirement.
    /// It is triggered by annotation on MVC action and setup in startup.
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="requirement">The requirement</param>
    /// <returns>A Task</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DenyAuthenticationMethodRequirement requirement)
    {
        string? userAuthenticationMethod = context.User.Claims.Where(
            c => c.Type.Equals(AuthenticationMethodClaimName)).Select(c => c.Value).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userAuthenticationMethod))
        {
            context.Fail();
        }
        else if (userAuthenticationMethod.Equals(
            requirement.AuthenticationMethod, System.StringComparison.OrdinalIgnoreCase))
        {
            context.Fail();
        }
        else
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
