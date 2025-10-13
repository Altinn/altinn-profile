namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Represents the different type of data that can be changed.
/// </summary>
public enum DataType
{
    /// <summary>
    /// The change happened to "party as user favorite".
    /// </summary>
    Favorites,

    /// <summary>
    /// The change happened to professional notification settings. Must use the same name as in A2.
    /// </summary>
    ReporteeNotificationSettings,

    /// <summary>
    /// The change happened to portal settings. Must use the same name as in A2.
    /// </summary>
    PortalSettingPreferences,
}
