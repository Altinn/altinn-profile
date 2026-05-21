using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Jobs
{
    /// <summary>
    /// A job that synchronizes changes in organization notification addresses.
    /// </summary>
    public partial class OrgSyncJob(IOrganizationNotificationAddressSyncJob orgUpdateJob, ILogger<OrgSyncJob> logger) : Job
    {
        private readonly IOrganizationNotificationAddressSyncJob _orgUpdateJob = orgUpdateJob;
        private readonly ILogger<OrgSyncJob> _logger = logger;

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _orgUpdateJob.SyncNotificationAddressesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the background synchronization.");
            }
        }
    }
}
