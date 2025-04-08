using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Constants;
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
        private const string _defaultIssuer = "Altinn";
        private const string _defaultType = "string";
        private const string _organizationNumberParam = "organizationNumber"; 

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

            XacmlJsonRequestRoot request = CreateDecisionRequest(context, requirement, httpContext.GetRouteData());

            XacmlJsonResponse response = await _pdp.GetDecisionForRequest(request);

            if (response?.Response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (DecisionHelper.ValidatePdpDecision(response.Response, context.User))
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }

        private static XacmlJsonRequestRoot CreateDecisionRequest(AuthorizationHandlerContext context, ResourceAccessRequirement requirement, RouteData routeData)
        {
            XacmlJsonRequest request = new()
            {
                AccessSubject = [],
                Action = [],
                Resource = []
            };

            string orgNumber = routeData.Values[_organizationNumberParam] as string;

            request.AccessSubject.Add(DecisionHelper.CreateSubjectCategory(context.User.Claims));
            request.Action.Add(DecisionHelper.CreateActionCategory(requirement.ActionType));

            if (!string.IsNullOrWhiteSpace(orgNumber))
            {
                request.Resource.Add(CreateResourceCategoryForResource(requirement.ResourceId, orgNumber));
            }
            else
            {
                throw new ArgumentException("invalid organization number " + orgNumber);
            }

            XacmlJsonRequestRoot jsonRequest = new() { Request = request };

            return jsonRequest;
        }

        private static XacmlJsonCategory CreateResourceCategoryForResource(string resourceid, string organizationnumber, bool includeResult = false)
        {
            XacmlJsonCategory resourceCategory = new()
            {
                Attribute =
            [
                CreateXacmlJsonAttribute(AltinnXacmlUrns.OrganizationNumber, organizationnumber, _defaultType, _defaultIssuer, includeResult),
            ]
            };

            if (!string.IsNullOrWhiteSpace(resourceid))
            {
                resourceCategory.Attribute.Add(CreateXacmlJsonAttribute(AltinnXacmlUrns.ResourceId, resourceid, _defaultType, _defaultIssuer));
            }

            return resourceCategory;
        }

        /// <summary>
        /// Create a new <see cref="XacmlJsonAttribute"/> with the given values.
        /// </summary>
        /// <param name="attributeId">The attribute id</param>
        /// <param name="value">The attribute value</param>
        /// <param name="dataType">The datatype for the attribute value</param>
        /// <param name="issuer">The issuer</param>
        /// <param name="includeResult">A value indicating whether the value should be included in the result.</param>
        /// <returns>A new created attribute</returns>
        public static XacmlJsonAttribute CreateXacmlJsonAttribute(string attributeId, string value, string dataType, string issuer, bool includeResult = false)
        {
            XacmlJsonAttribute xacmlJsonAttribute = new()
            {
                AttributeId = attributeId,
                Value = value,
                DataType = dataType,
                Issuer = issuer,
                IncludeInResult = includeResult
            };

            return xacmlJsonAttribute;
        }
    }
}
