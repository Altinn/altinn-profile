namespace Altinn.Profile.Jobs
{
    /// <summary>
    /// Lease names for jobs.
    /// </summary>
    internal static class JobLeaseNames
    {
        /// <summary>
        /// Lease name for <see cref="KrrSyncJob"/>.
        /// </summary>
        internal const string KrrSyncJob = "krr-sync-job";

        /// <summary>
        /// Lease name for <see cref="OrgSyncJob"/>.
        /// </summary>
        internal const string OrgSyncJob = "org-sync-job";
    }
}
