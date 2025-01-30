using System;
using System.Collections.Generic;
using System.Security.Claims;

using Altinn.Common.AccessToken.Constants;

using AltinnCore.Authentication.Constants;

namespace Altinn.Profile.Tests.IntegrationTests.Utils;

public static class PrincipalUtil
{
    public static string GetToken(int userId, int authenticationLevel = 2)
    {
        List<Claim> claims = [];
        string issuer = "www.altinn.no";
        claims.Add(new Claim(AltinnCoreClaimTypes.UserId, userId.ToString(), ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.UserName, "UserOne", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.PartyID, userId.ToString(), ClaimValueTypes.Integer32, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, authenticationLevel.ToString(), ClaimValueTypes.Integer32, issuer));

        ClaimsIdentity identity = new("mock");
        identity.AddClaims(claims);

        ClaimsPrincipal principal = new(identity);
        string token = JwtGenerator.GenerateToken(principal, new TimeSpan(1, 1, 1));

        return token;
    }

    public static string GetAccessToken(string issuer, string app)
    {
        List<Claim> claims = [new Claim(AccessTokenClaimTypes.App, app, ClaimValueTypes.String, issuer)];
        
        ClaimsIdentity identity = new("mock");
        identity.AddClaims(claims);

        ClaimsPrincipal principal = new(identity);
        string token = JwtGenerator.GenerateToken(principal, new TimeSpan(0, 1, 5), issuer);

        return token;
    }

    public static string GetOrgToken(string org, int authenticationLevel = 4)
    {
        List<Claim> claims = [];
        string issuer = "www.altinn.no";
        claims.Add(new Claim(AltinnCoreClaimTypes.Org, org, ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, authenticationLevel.ToString(), ClaimValueTypes.Integer32, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.OrgNumber, "orgno", ClaimValueTypes.Integer32, issuer));

        return GenerateToken(claims);
    }

    public static string GetSystemUserToken(Guid systemUserId)
    {
        List<Claim> claims = [];
        string issuer = "www.altinn.no";
        string systemUser = $$"""
            {
                "type": "urn:altinn:systemuser",
                "systemuser_org": {
                    "authority": "iso6523-actorid-upis",
                    "ID": "myOrg"
                },
                "systemuser_id":["{{systemUserId}}"],
                "system_id": "the_matrix"
            }
        """;
        claims.Add(new Claim("authorization_details", systemUser, ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer));
        claims.Add(new Claim(AltinnCoreClaimTypes.AuthenticationLevel, "3", ClaimValueTypes.Integer32, issuer));

        return GenerateToken(claims);
    }

    public static string GetInvalidSystemUserToken(Guid systemUserId)
    {
        List<Claim> claims = [];
        string issuer = "www.altinn.no";
        string systemUser = "not a valid authorization_details claim";
        claims.Add(new Claim("authorization_details", systemUser, ClaimValueTypes.String, issuer));

        return GenerateToken(claims);
    }

    private static string GenerateToken(List<Claim> claims)
    {
        ClaimsIdentity identity = new("mock");
        identity.AddClaims(claims);

        ClaimsPrincipal principal = new(identity);
        string token = JwtGenerator.GenerateToken(principal, new TimeSpan(1, 1, 1));

        return token;
    }
}
