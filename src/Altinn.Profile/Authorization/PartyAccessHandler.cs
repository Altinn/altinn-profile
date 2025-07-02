using System;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// AuthorizationHandler that is created for determining if a user has an association with a given party.
    /// Authorizes based om PartyAccessRequirement and partyUuid from route
    /// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
    /// in asp.net core
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PartyAccessHandler"/> class.
    /// </remarks>
    /// <param name="httpContextAccessor">The http context accessor</param>
    /// <param name="authClient">The client to access authorization api</param>
    /// <param name="registerClient">The client to access register api</param>
    public class PartyAccessHandler(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationClient authClient, IRegisterClient registerClient) : AuthorizationHandler<PartyAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IAuthorizationClient _authorizationClient = authClient;
        private readonly IRegisterClient _registerClient = registerClient;
        private const string _partyUuid = "partyUuid";

        /// <summary>
        /// This method authorize access bases on context and requirement
        /// Is triggered by annotation on MVC action and setup in startup.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="requirement">The requirement</param>
        /// <returns>A Task</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PartyAccessRequirement requirement)
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            var routeData = httpContext.GetRouteData();

            if (routeData?.Values[_partyUuid] is not string partyUuidString || !Guid.TryParse(partyUuidString, out Guid partyUuid))
            {
                 context.Fail();
                 return;
            }

            var partyId = await _registerClient.GetPartyId(partyUuid, CancellationToken.None);

            if (partyId == null)
            {
                context.Fail();
                return;
            }

            var error = ClaimsHelper.TryGetUserIdFromClaims(httpContext, out int userId);
            if (error != null)
            {
                context.Fail();
                return;
            }

            bool valid = await _authorizationClient.ValidateSelectedParty(userId, (int)partyId, CancellationToken.None);

            if (valid)
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }
    }
}
