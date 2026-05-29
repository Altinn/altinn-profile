using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Integrations.ContactRegister;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Jobs
{
    /// <summary>
    /// A job that synchronizes the changes in the contact details for persons.
    /// </summary>
    /// <param name="contactRegisterUpdateJob">Service that performs contact register synchronization.</param>
    /// <param name="logger">Logger used for job execution and failure reporting.</param>
    public partial class KrrSyncJob(IContactRegisterUpdateJob contactRegisterUpdateJob, ILogger<KrrSyncJob> logger) : Job
    {
        private readonly IContactRegisterUpdateJob _contactRegisterUpdateJob = contactRegisterUpdateJob;
        private readonly ILogger<KrrSyncJob> _logger = logger;

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _contactRegisterUpdateJob.SyncContactInformationAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the background synchronization.");
            }
        }
    }
}
