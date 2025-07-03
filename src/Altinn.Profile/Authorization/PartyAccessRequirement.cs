using Microsoft.AspNetCore.Authorization;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// Requirement for authorization policies used for accessing parties for a user.
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core.
    /// </summary>
    public class PartyAccessRequirement : IAuthorizationRequirement
    {
    }
}
