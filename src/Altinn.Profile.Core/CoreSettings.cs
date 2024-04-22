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
}
