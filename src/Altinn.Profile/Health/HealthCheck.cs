using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Altinn.Profile.Health
{
    /// <summary>
    /// Health check service configured in startup
    /// Listen to 
    /// </summary>
    public class HealthCheck : IHealthCheck
    {
        /// <summary>
        /// Verifies the health status
        /// </summary>
        /// <param name="context">The healthcheck context</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A health result</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
        }
    }
}
