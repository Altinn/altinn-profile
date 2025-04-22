using System;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Constants;
using Altinn.Common.PEP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// Represents a collection of helper methods for creating a decision request
    /// </summary>
    public static class AuthorizationHelper
    {
        private const string _defaultIssuer = "Altinn";
        private const string _defaultType = "string";
        private const string _organizationNumberParam = "organizationNumber";

        /// <summary>
        /// Creates a decision request based on input
        /// </summary>
        /// <returns>Returns a XacmlJsonRequestRoot with data </returns>
        public static XacmlJsonRequestRoot CreateDecisionRequest(AuthorizationHandlerContext context, ResourceAccessRequirement requirement, RouteData routeData)
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
                throw new InvalidOperationException("invalid organization number for this operation ");
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
