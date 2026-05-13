using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Common.PEP.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// AuthorizationHandler that is created for handling access to api with feature toggled scopes.
    /// The requirement is fulfilled if the user has the scope specified in the requirement and the feature toggle for that scope is enabled.
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FeatureToggledScopeAccessHandler"/> class.
    /// </remarks>
    public class FeatureToggledScopeAccessHandler(
        IOptions<PortalAccessSettings> portalAccessSettings,
        ScopeAccessHandler scopeAccessHandler,
        ILogger<FeatureToggledScopeAccessHandler> logger) : AuthorizationHandler<FeatureToggledScopeAccessRequirement>
    {
        private readonly PortalAccessSettings _portalAccessSettings = portalAccessSettings.Value;

        private readonly ScopeAccessHandler _scopeAccessHandler = scopeAccessHandler;

        private readonly ILogger<FeatureToggledScopeAccessHandler> _logger = logger;

        /// <summary>
        /// This method authorizes access based on context and requirement.
        /// It is triggered by annotation on MVC action and setup in startup.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="requirement">The requirement</param>
        /// <returns>A Task</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, FeatureToggledScopeAccessRequirement requirement)
        {
            if (_portalAccessSettings.EnforceAccessCheck)
            {
                await _scopeAccessHandler.HandleAsync(context);
                return;
            }

            string contextScope = context.User?.Identities
                ?.FirstOrDefault(i => i.AuthenticationType != null && i.AuthenticationType.Equals("AuthenticationTypes.Federation"))?.Claims
                .Where(c => c.Type.Equals("urn:altinn:scope"))
                .Select(c => c.Value).FirstOrDefault();

            contextScope ??= context.User?.Claims.Where(c => c.Type.Equals("scope")).Select(c => c.Value).FirstOrDefault();

            bool validScope = false;

            if (!string.IsNullOrWhiteSpace(contextScope))
            {
                string[] requiredScopes = requirement.Scope;
                List<string> clientScopes = contextScope.Split(' ').ToList();

                validScope = requiredScopes.Any(clientScopes.Contains);
            }

            if (!validScope)
            {
                _logger.LogWarning("Access should be denied. Required scope {RequiredScope} not found in user claims. Found scopes {FoundScopes}", requirement.Scope, contextScope);
            }

            context.Succeed(requirement);
        }
    }
}
