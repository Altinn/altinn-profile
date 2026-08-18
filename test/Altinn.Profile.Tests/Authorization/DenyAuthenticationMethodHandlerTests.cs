#nullable enable

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Altinn.Profile.Authorization;

using Microsoft.AspNetCore.Authorization;

using Xunit;

namespace Altinn.Profile.Tests.Authorization;

public class DenyAuthenticationMethodHandlerTests
{
    private const string DeniedAuthenticationMethod = "IdportenEpost";
    private const string AllowedAuthenticationMethod = "IdportenBankId";

    [Fact]
    public async Task HandleAsync_UserHasADeniedAuthenticationMethod_Fails()
    {
        // Arrange
        ClaimsPrincipal user = CreatePrincipalWithAuthenticationMethod(DeniedAuthenticationMethod);

        var requirement = new DenyAuthenticationMethodRequirement(DeniedAuthenticationMethod);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        var target = new DenyAuthenticationMethodHandler();

        // Act
        await target.HandleAsync(context);

        // Assert
        Assert.True(context.HasFailed);
    }

    [Fact]
    public async Task HandleAsync_UserHasAnAllowedAuthenticationMethod_DoesNotFail()
    {
        // Arrange
        ClaimsPrincipal user = CreatePrincipalWithAuthenticationMethod(AllowedAuthenticationMethod);

        var requirement = new DenyAuthenticationMethodRequirement(DeniedAuthenticationMethod);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        var target = new DenyAuthenticationMethodHandler();

        // Act
        await target.HandleAsync(context);

        // Assert
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleAsync_UserHasNoAuthenticationMethodClaim_Fails()
    {
        // Arrange
        ClaimsPrincipal user = new(new ClaimsIdentity());

        var requirement = new DenyAuthenticationMethodRequirement(DeniedAuthenticationMethod);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        var target = new DenyAuthenticationMethodHandler();

        // Act
        await target.HandleAsync(context);

        // Assert
        Assert.True(context.HasFailed);
    }

    private static ClaimsPrincipal CreatePrincipalWithAuthenticationMethod(string authenticationMethod)
    {
        List<Claim> claims = [new("urn:altinn:authenticatemethod", authenticationMethod)];
        ClaimsIdentity identity = new(claims);
        return new ClaimsPrincipal(identity);
    }
}
