namespace Altinn.Profile.Changelog
{
    /// <summary>
    /// Represents the settings for the changelog import jobs.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public class ImportJobSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the favorites import is enabled.
        /// </summary>
        public bool FavoritesImportEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the notification settings import is enabled.
        /// </summary>
        public bool NotificationSettingsImportEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the portal settings import is enabled.
        /// </summary>
        public bool ProfileSettingsImportEnabled { get; set; } = false;
    }
}
