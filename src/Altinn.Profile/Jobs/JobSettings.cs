namespace Altinn.Profile.Jobs
{
    /// <summary>
    /// Represents the settings for the jobs.
    /// </summary>
    public class JobSettings
    {
        /// <summary>
        /// A value indicating whether the KRR synchronization is enabled.
        /// </summary>
        public bool KrrSyncEnabled { get; set; } = false;

        /// <summary>
        /// A value indicating the wait time in minutes between each synchronization for KRR.
        /// </summary>
        public uint KrrSyncWaitTimeInMinutes { get; set; } = 10;

        /// <summary>
        /// A value indicating whether the organization synchronization is enabled.
        /// </summary>
        public bool OrgSyncEnabled { get; set; } = false;

        /// <summary>
        /// A value indicating the wait time in minutes between each synchronization for the organization notification settings.
        /// </summary>
        public uint OrgSyncWaitTimeInMinutes { get; set; } = 10;
    }
}
