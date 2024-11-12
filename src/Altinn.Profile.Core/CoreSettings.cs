namespace Altinn.Profile.Core;

/// <summary>
/// General configuration settings for the core project
/// </summary>
public class CoreSettings
{
    /// <summary>
    /// Feature flag that dictates whether KRR data should be fetched locally (from our own copy) or remotely (from Altinn 2)
    /// </summary>
    public bool EnableLocalKrrFetch { get; set; } = false;

    /// <summary>
    /// The number of seconds the user profile will be kept in the cache
    /// </summary>
    public int ProfileCacheLifetimeSeconds { get; set; } = 600;
}
