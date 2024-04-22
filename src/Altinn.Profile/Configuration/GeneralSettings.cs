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
}
