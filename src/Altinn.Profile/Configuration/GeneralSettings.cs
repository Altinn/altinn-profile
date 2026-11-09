namespace Altinn.Profile.Configuration;

/// <summary>
/// General configuration settings
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// Open Id Connect Well known endpoint
    /// </summary>
    public string OpenIdWellKnownEndpoint { get; set; }

    /// <summary>
    /// Name of the cookie for where JWT is stored
    /// </summary>
    public string JwtCookieName { get; set; }

    /// <summary>
    /// Folder with the public certificates (<c>{issuer}-org.pem</c>) used to validate platform access tokens
    /// when Key Vault is not configured, e.g. when running locally with docker compose.
    /// </summary>
    public string AccessTokenPublicCertsFolder { get; set; }
}
