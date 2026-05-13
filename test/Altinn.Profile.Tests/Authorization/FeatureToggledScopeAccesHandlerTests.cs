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
    public class FeatureToggledScopeAccesHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsFalse_AndRequiredScopeIsPresent_SucceedsWithoutDelegating()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccesRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("altinn:portal/enduser other-scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var scopeAccessHandler = new TestScopeAccessHandler(shouldSucceed: false);
            var handler = CreateHandler(enforceAccessCheck: false, scopeAccessHandler);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
            Assert.False(scopeAccessHandler.WasInvoked);
        }

        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsFalse_AndRequiredScopeIsMissing_SucceedsWithoutDelegating()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccesRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("some:other.scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var scopeAccessHandler = new TestScopeAccessHandler(shouldSucceed: false);
            var handler = CreateHandler(enforceAccessCheck: false, scopeAccessHandler);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
            Assert.False(scopeAccessHandler.WasInvoked);
        }

        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsTrue_AndDelegatedHandlerSucceeds_SucceedsAndDelegates()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccesRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("altinn:portal/enduser other-scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var scopeAccessHandler = new TestScopeAccessHandler(shouldSucceed: true);
            var handler = CreateHandler(enforceAccessCheck: true, scopeAccessHandler);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
            Assert.True(scopeAccessHandler.WasInvoked);
        }

        [Fact]
        public async Task HandleAsync_WhenEnforceAccessCheckIsTrue_AndDelegatedHandlerFails_FailsAndDelegates()
        {
            // Arrange
            var requirement = new FeatureToggledScopeAccesRequirement("altinn:portal/enduser");
            ClaimsPrincipal user = CreatePrincipalWithFederationScope("some:other.scope");
            var context = new AuthorizationHandlerContext([requirement], user, null);
            var scopeAccessHandler = new TestScopeAccessHandler(shouldSucceed: false);
            var handler = CreateHandler(enforceAccessCheck: true, scopeAccessHandler);

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            Assert.True(context.HasFailed);
            Assert.True(scopeAccessHandler.WasInvoked);
        }

        private static FeatureToggledScopeAccesHandler CreateHandler(bool enforceAccessCheck, ScopeAccessHandler scopeAccessHandler)
        {
            IOptions<AccessSettings> options = Options.Create(new AccessSettings
            {
                EnforceAccessCheck = enforceAccessCheck
            });

            return new FeatureToggledScopeAccesHandler(options, scopeAccessHandler, NullLogger<FeatureToggledScopeAccesHandler>.Instance);
        }

        private static ClaimsPrincipal CreatePrincipalWithFederationScope(string scope)
        {
            List<Claim> claims = [new("urn:altinn:scope", scope)];
            ClaimsIdentity identity = new(claims, "AuthenticationTypes.Federation");
            return new ClaimsPrincipal(identity);
        }

        private sealed class TestScopeAccessHandler : ScopeAccessHandler
        {
            private readonly bool _shouldSucceed;

            public TestScopeAccessHandler(bool shouldSucceed)
            {
                _shouldSucceed = shouldSucceed;
            }

            public bool WasInvoked { get; private set; }

            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IScopeAccessRequirement requirement)
            {
                WasInvoked = true;

                if (_shouldSucceed)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }

                return Task.CompletedTask;
            }
        }
    }
}
