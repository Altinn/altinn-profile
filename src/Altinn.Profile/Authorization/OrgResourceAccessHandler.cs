using System;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Helpers;
using Altinn.Common.PEP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// AuthorizationHandler that is created for handling access to api with organizationNumber in path.
    /// Authorizes based om ResourceAccessRequirement and organization number from route
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ResourceAccessHandler"/> class.
    /// </remarks>
    /// <param name="httpContextAccessor">The http context accessor</param>
    /// <param name="pdp">The pdp</param>
    public class OrgResourceAccessHandler(
        IHttpContextAccessor httpContextAccessor,
        IPDP pdp) : AuthorizationHandler<ResourceAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IPDP _pdp = pdp;

        /// <summary>
        /// This method authorize access bases on context and requirement
        /// Is triggered by annotation on MVC action and setup in startup.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="requirement">The requirement</param>
        /// <returns>A Task</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceAccessRequirement requirement)
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;

            XacmlJsonRequestRoot request = AuthorizationHelper.CreateDecisionRequest(context, requirement, httpContext.GetRouteData());

            XacmlJsonResponse response = await _pdp.GetDecisionForRequest(request);

            if (response?.Response == null)
            {
                throw new NullReferenceException("response");
            }

            if (DecisionHelper.ValidatePdpDecision(response.Response, context.User))
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }
    }
}
