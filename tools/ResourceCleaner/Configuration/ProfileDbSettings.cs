namespace ResourceCleaner.Configuration;

/// <summary>
/// Settings for the Profile Postgres database
/// </summary>
public class ProfileDbSettings
{
    /// <summary>
    /// Connection string for the Profile postgres db
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Command timeout in seconds. Defaults to 360 (6 minutes).
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 360;
}
