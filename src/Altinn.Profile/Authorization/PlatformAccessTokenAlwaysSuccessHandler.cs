using System.Threading.Tasks;

using Altinn.Common.AccessToken;

using Microsoft.AspNetCore.Authorization;

namespace Altinn.Profile.Authorization;

/// <summary>
/// An authorization handler that always succeeds for any access token requirement.
/// </summary>
/// <remarks>This handler is used to automatically succeed any authorization requirement that implements <see
/// cref="IAccessTokenRequirement"/>. It is typically used in scenarios where access token validation is not required or
/// is handled by other means.</remarks>
public class PlatformAccessTokenAlwaysSuccessHandler : AuthorizationHandler<IAccessTokenRequirement>
{
    /// <summary>
    /// Handles the authorization requirement by always succeeding it.
    /// </summary>
    /// <param name="context">The current authorization handler context.</param>
    /// <param name="requirement">The requirement for the given operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IAccessTokenRequirement requirement)
    {
        context.Succeed(requirement);
        await Task.CompletedTask;
    }
}
