using Microsoft.AspNetCore.Authorization;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// Requirement for authorization policies used for accessing endpoints protected by feature toggled scopes. 
    /// The requirement is fulfilled if the user has the scope specified in the requirement and the feature toggle for that scope is enabled.
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FeatureToggledScopeAccessRequirement"/> class and 
    /// pupulates the Scope property with the given scope.
    /// </remarks>
    /// <param name="scope">The scope for this requirement</param>
    public class FeatureToggledScopeAccessRequirement(string scope) : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets or sets the scope defined for the policy using this requirement
        /// </summary>
        public string[] Scope { get; set; } = [scope];
    }
}
