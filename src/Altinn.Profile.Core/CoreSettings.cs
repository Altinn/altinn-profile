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
    /// A flag indicating whether to perform a lookup for a preselected party ID at the register when fetching the user profile. If not, the value from SBLBridge must be provided.
    /// </summary>
    public bool LookupPreselectedPartyIdAtRegister { get; set; }

    /// <summary>
    /// A flag indicating whether to perform a lookup for users from the register and compare this.
    /// </summary>
    public bool RegisterLookupInShadowMode { get; set; }

    /// <summary>
    /// A flag indicating whether register should be primary source for user profiles, with fallback to legacy source.
    /// </summary>
    public bool RegisterAsPrimaryUserProfileSource { get; set; }
}
