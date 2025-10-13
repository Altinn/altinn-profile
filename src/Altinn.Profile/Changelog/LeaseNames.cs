namespace Altinn.Profile.Changelog
{
    /// <summary>
    /// Lease names for register.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    internal static class LeaseNames
    {
        /// <summary>
        /// Lease name for <see cref="FavoriteImportJob"/>.
        /// </summary>
        internal const string A2FavoriteImport = "a2-favorites-import";

        /// <summary>
        /// Lease name for <see cref="NotificationSettingImportJob"/>.
        /// </summary>
        internal const string A2NotificationSettingImport = "a2-notification-settings-import";

        /// <summary>
        /// Lease name for <see cref="ProfileSettingImportJob"/>.
        /// </summary>
        internal const string A2ProfileSettingImport = "a2-portal-settings-import";
    }
}
