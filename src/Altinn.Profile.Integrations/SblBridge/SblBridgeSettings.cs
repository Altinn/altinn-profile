namespace Altinn.Profile.Integrations.SblBridge;

/// <summary>
/// Class containing all configuration settings for SBL Bridge
/// </summary>
public class SblBridgeSettings
{
    /// <summary>
    /// Gets or sets the SBL Bridge Profile API endpoint
    /// </summary>
    public string ApiProfileEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// A feature flag indicating whether to update A2 when updating favorites
    /// </summary>
    public bool UpdateA2Favorites { get; set; }

    /// <summary>
    /// A feature flag indicating whether to update A2 when updating notification settings
    /// </summary>
    public bool UpdateA2NotificationSettings { get; set; }

    /// <summary>
    /// A feature flag indicating whether to update A2 when updating portal setting preferences
    /// </summary>
    public bool UpdateA2PortalSettings { get; set; }
}
