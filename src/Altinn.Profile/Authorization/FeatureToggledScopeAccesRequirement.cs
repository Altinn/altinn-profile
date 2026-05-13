using Altinn.Common.PEP.Authorization;

using Microsoft.AspNetCore.Authorization;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// Requirement for authorization policies used for accessing endpoints protected by feature toggled scopes. 
    /// The requirement is fulfilled if the user has the scope specified in the requirement and the feature toggle for that scope is enabled.
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core.
    /// </summary>
    public class FeatureToggledScopeAccesRequirement(string scope) : ScopeAccessRequirement(scope)
    {
    }
}
