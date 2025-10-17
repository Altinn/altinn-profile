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
    /// Feature flag to lookup unit contact points at SBL bridge
    /// </summary>
    public bool LookupUnitContactPointsAtSblBridge { get; set; }
}
