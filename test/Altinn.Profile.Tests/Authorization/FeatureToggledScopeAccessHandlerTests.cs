using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Altinn.Common.PEP.Authorization;
using Altinn.Profile.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Xunit;

namespace Altinn.Profile.Tests.Authorization
{
    public class FeatureToggledScopeAccessHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsFalse_AndRequiredScopeIsPresent_SucceedsWithoutDelegating()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccessRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("altinn:portal/enduser other-scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var handler = CreateHandler(enforceAccessCheck: false);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsFalse_AndRequiredScopeIsMissing_SucceedsWithoutDelegating()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccessRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("some:other.scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var handler = CreateHandler(enforceAccessCheck: false);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsTrue_AndDelegatedHandlerSucceeds_SucceedsAndDelegates()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccessRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("altinn:portal/enduser other-scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var handler = CreateHandler(enforceAccessCheck: true);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsTrue_AndDelegatedHandlerDoesNotSucceed_DoesNotSucceedAndDelegates()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccessRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("some:other.scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var handler = CreateHandler(enforceAccessCheck: true);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        private static FeatureToggledScopeAccessHandler CreateHandler(bool enforceAccessCheck)
        {
            IOptions<PortalAccessSettings> options = Options.Create(new PortalAccessSettings
            {
                EnforceAccessCheck = enforceAccessCheck
            });

            return new FeatureToggledScopeAccessHandler(options, NullLogger<FeatureToggledScopeAccessHandler>.Instance);
        }

        private static ClaimsPrincipal CreatePrincipalWithFederationScope(string scope)
        {
            List<Claim> claims = [new("scope", scope)];
            ClaimsIdentity identity = new(claims);
            return new ClaimsPrincipal(identity);
        }
    }
}
