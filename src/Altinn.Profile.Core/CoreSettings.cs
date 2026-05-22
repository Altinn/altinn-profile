namespace Altinn.Profile.Core;

/// <summary>
/// General configuration settings for the core project
/// </summary>
public class CoreSettings
{
    /// <summary>
    /// The number of seconds the user profile will be kept in the cache
    /// </summary>
    public int ProfileCacheLifetimeSeconds { get; set; } = 600;

    /// <summary>
    /// A flag indicating whether register should be primary source for user profiles, with fallback to legacy source.
    /// </summary>
    public bool RegisterAsPrimaryUserProfileSource { get; set; }

    /// <summary>
    /// A flag indicating whether to perform a lookup for users from the SBL bridge as a fallback if not found in Register.
    /// </summary>
    public bool SblBridgeFallbackEnabled { get; set; } = true;
}
