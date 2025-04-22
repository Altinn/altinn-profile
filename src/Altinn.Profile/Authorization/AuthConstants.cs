namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// Constants for authorization policies
    /// </summary>
    public static class AuthConstants
    {
        /// <summary>
        /// Policy name for platform access
        /// </summary>
        public const string PlatformAccess = "PlatformAccess";

        /// <summary>
        /// Policy name for reading organization notification addresses
        /// </summary>
        public const string OrgNotificationAddress_Read = "OrgNotificationAddress_Read";

        /// <summary>
        /// Policy name for writing organization notification addresses
        /// </summary>
        public const string OrgNotificationAddress_Write = "OrgNotificationAddress_Write";
    }
}
