#nullable enable

using Microsoft.AspNetCore.Authorization;

namespace Altinn.Profile.Authorization;

/// <summary>
/// Authorization requirement to deny access to endpoint if the user was authenticated using a specific method.
/// </summary>
/// <param name="authenticationMethod">The authentication method to deny.</param>
public class DenyAuthenticationMethodRequirement(string authenticationMethod) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets or sets the authentication method defined for the policy using this requirement.
    /// </summary>
    public string AuthenticationMethod { get; set; } = authenticationMethod;
}
