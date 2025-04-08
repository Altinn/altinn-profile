using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Constants;
using Altinn.Common.PEP.Helpers;
using Altinn.Common.PEP.Interfaces;
using Altinn.Common.PEP.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// AuthorizationHandler that is created for handling access to api.
    /// Authorizes based om ResourceAccessRequirement and organization number from route
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core
    /// </summary>
    public class ApiResourceAccessHandler : AuthorizationHandler<ResourceAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPDP _pdp;
        private readonly ILogger _logger;
        private const string DefaultIssuer = "Altinn";
        private const string DefaultType = "string";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAccessHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The http context accessor</param>
        /// <param name="pdp">The pdp</param>
        /// <param name="logger">The logger. </param>
        public ApiResourceAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            IPDP pdp,
            ILogger<ResourceAccessHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _pdp = pdp;
            _logger = logger;
        }

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

            XacmlJsonRequestRoot request = CreateDecisionRequest(context, requirement, _httpContextAccessor.HttpContext.GetRouteData(), _httpContextAccessor.HttpContext.Request.Headers);

            XacmlJsonResponse response = await _pdp.GetDecisionForRequest(request);

            if (response?.Response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (!DecisionHelper.ValidatePdpDecision(response.Response, context.User))
            {
                context.Fail();
            }

            context.Succeed(requirement);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a decision request based on input
        /// </summary>
        /// <returns></returns>
        public static XacmlJsonRequestRoot CreateDecisionRequest(AuthorizationHandlerContext context, ResourceAccessRequirement requirement, RouteData routeData, IHeaderDictionary headers)
        {
            XacmlJsonRequest request = new XacmlJsonRequest();
            request.AccessSubject = new List<XacmlJsonCategory>();
            request.Action = new List<XacmlJsonCategory>();
            request.Resource = new List<XacmlJsonCategory>();

            string orgNumber = routeData.Values["organizationNumber"] as string;

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

            XacmlJsonRequestRoot jsonRequest = new XacmlJsonRequestRoot() { Request = request };

            return jsonRequest;
        }

        private static XacmlJsonCategory CreateResourceCategoryForResource(string resourceid, string organizationnumber, bool includeResult = false)
        {
            XacmlJsonCategory resourceCategory = new XacmlJsonCategory();
            resourceCategory.Attribute = new List<XacmlJsonAttribute>();

            resourceCategory.Attribute.Add(CreateXacmlJsonAttribute(AltinnXacmlUrns.OrganizationNumber, organizationnumber, DefaultType, DefaultIssuer, includeResult));

            if (!string.IsNullOrWhiteSpace(resourceid))
            {
                resourceCategory.Attribute.Add(CreateXacmlJsonAttribute(AltinnXacmlUrns.ResourceId, resourceid, DefaultType, DefaultIssuer));
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
            XacmlJsonAttribute xacmlJsonAttribute = new XacmlJsonAttribute();

            xacmlJsonAttribute.AttributeId = attributeId;
            xacmlJsonAttribute.Value = value;
            xacmlJsonAttribute.DataType = dataType;
            xacmlJsonAttribute.Issuer = issuer;
            xacmlJsonAttribute.IncludeInResult = includeResult;

            return xacmlJsonAttribute;
        }
    }
}
