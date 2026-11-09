#!/usr/bin/env dotnet
#:package Microsoft.IdentityModel.JsonWebTokens@8.22.0

// Generates a JWT that the local Profile container accepts.
// - user/org tokens are signed with test/Altinn.Profile.Tests/jwtselfsignedcert.pfx, whose public key the
//   WireMock container publishes as the platform authentication JWKS. Send as "Authorization: Bearer <token>".
// - platform tokens are signed with test/Altinn.Profile.Tests/ttd-org.pfx, whose public cert docker-compose.yml
//   mounts as /certs/public/{platform,ttd}-org.pem. Send as "PlatformAccessToken: <token>".
//
//   dotnet run local-dev/token.cs                                  # end user (test person)
//   dotnet run local-dev/token.cs -- user --user-id 20462603 --party-id 50891883 --scope "altinn:portal/enduser"
//   dotnet run local-dev/token.cs -- org --org-number 313605590 --scope "altinn:profile.support.admin"
//   dotnet run local-dev/token.cs -- platform --app notifications
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var options = ParseArgs(args);
string kind = options.GetValueOrDefault("kind", "user");

var claims = new Dictionary<string, object>
{
    ["urn:altinn:authlevel"] = options.GetValueOrDefault("auth-level", "3"),
};

switch (kind)
{
    case "user":
        claims["urn:altinn:userid"] = options.GetValueOrDefault("user-id", "20462603");
        claims["urn:altinn:partyid"] = options.GetValueOrDefault("party-id", "50891883");
        claims["urn:altinn:username"] = options.GetValueOrDefault("username", "localdev");
        claims["urn:altinn:authenticatemethod"] = options.GetValueOrDefault("auth-method", "BankID");
        claims["scope"] = options.GetValueOrDefault("scope", "altinn:portal/enduser");
        break;
    case "org":
        claims["urn:altinn:org"] = options.GetValueOrDefault("org", "ttd");
        claims["urn:altinn:orgNumber"] = options.GetValueOrDefault("org-number", "313605590");
        claims["urn:altinn:authenticatemethod"] = "maskinporten";
        claims["scope"] = options.GetValueOrDefault("scope", string.Empty);
        break;
    case "platform":
        // PlatformAccessToken (header "PlatformAccessToken"), as sent by other platform components and apps.
        claims.Clear();
        claims["urn:altinn:app"] = options.GetValueOrDefault("app", "notifications");
        break;
    default:
        Console.Error.WriteLine($"Unknown token kind '{kind}'. Use 'user', 'org' or 'platform'.");
        return 1;
}

string testDir = Path.Combine(FindRepoRoot(), "test", "Altinn.Profile.Tests");
using X509Certificate2 cert = kind == "platform"
    ? X509CertificateLoader.LoadPkcs12FromFile(Path.Combine(testDir, "ttd-org.pfx"), string.Empty)
    : X509CertificateLoader.LoadPkcs12FromFile(Path.Combine(testDir, "jwtselfsignedcert.pfx"), "qwer1234");

var descriptor = new SecurityTokenDescriptor
{
    Issuer = kind == "platform" ? options.GetValueOrDefault("issuer", "platform") : "http://mocks:8080/authentication/api/v1/openid/",
    Audience = "altinn.no",
    Claims = claims,
    Expires = DateTime.UtcNow.AddMinutes(int.Parse(options.GetValueOrDefault("minutes", "480"))),
    SigningCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256),
};

Console.WriteLine(new JsonWebTokenHandler().CreateToken(descriptor));
return 0;

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>();
    int i = 0;
    if (args.Length > 0 && !args[0].StartsWith("--"))
    {
        result["kind"] = args[0];
        i = 1;
    }

    for (; i < args.Length - 1; i += 2)
    {
        result[args[i].TrimStart('-')] = args[i + 1];
    }

    return result;
}

static string FindRepoRoot()
{
    string? dir = AppContext.GetData("EntryPointFileDirectoryPath") as string ?? Directory.GetCurrentDirectory();
    while (dir is not null && !File.Exists(Path.Combine(dir, "Altinn.Profile.sln")))
    {
        dir = Path.GetDirectoryName(dir);
    }

    return dir ?? throw new InvalidOperationException("Could not find the repository root (Altinn.Profile.sln).");
}
