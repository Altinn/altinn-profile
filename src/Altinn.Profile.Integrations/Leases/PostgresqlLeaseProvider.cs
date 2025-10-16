#nullable enable

using Altinn.Authorization.ServiceDefaults.Leases;
using Altinn.Profile.Integrations.Repositories.A2Sync;

using Microsoft.Extensions.Logging;

using Npgsql;

using Polly;
using Polly.Retry;

namespace Altinn.Profile.Integrations.Leases;

/// <summary>
/// Implementation of <see cref="ILeaseProvider"/> that uses a postgresql database
/// as lease storage.
/// </summary>
/// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
public partial class PostgresqlLeaseProvider
    : ILeaseProvider
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PostgresqlLeaseProvider> _logger;
    private readonly ResiliencePipeline<LeaseAcquireResult> _retryLeasePipeline;
    private readonly ILeaseRepository _leaseRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresqlLeaseProvider"/> class.
    /// </summary>
    public PostgresqlLeaseProvider(
        TimeProvider timeProvider,
        ILogger<PostgresqlLeaseProvider> logger,
        ILeaseRepository leaseRepository)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _leaseRepository = leaseRepository;

        var pipelineBuilder = new ResiliencePipelineBuilder<LeaseAcquireResult>();
        pipelineBuilder.TimeProvider = timeProvider;
        pipelineBuilder.AddRetry(new RetryStrategyOptions<LeaseAcquireResult>
        {
            ShouldHandle = new PredicateBuilder<LeaseAcquireResult>()
                .Handle<PostgresException>(e => e.SqlState == PostgresErrorCodes.SerializationFailure),
            BackoffType = DelayBackoffType.Constant,
            UseJitter = false,
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(10),
            OnRetry = args =>
            {
                Log.FailedToUpsertLeaseDueToSerializationError(logger);
                
                return ValueTask.CompletedTask;
            },
        });

        _retryLeasePipeline = pipelineBuilder.Build();
    }

    /// <inheritdoc/>
    public async Task<LeaseAcquireResult> TryAcquireLease(
        string leaseId,
        TimeSpan duration,
        Func<LeaseInfo, bool>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var expires = now + duration;
        var token = Guid.NewGuid();

        var lease = new Lease 
        {
            Id = leaseId,
            Token = token,
            Expires = expires,
            Acquired = now,
            Released = null, // Don't update Released when acquiring
        };
        var result = await UpsertLease(lease, now, filter, cancellationToken);

        if (result.IsLeaseAcquired)
        {
            Log.LeaseAcquired(_logger, result.Lease.LeaseId);
        }
        else
        {
            Log.LeaseNotAcquiredAlreadyHeld(_logger, leaseId);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<LeaseAcquireResult> TryRenewLease(LeaseTicket lease, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var expires = now + duration;
        var leaseId = lease.LeaseId;
        var token = lease.Token;

        var leaseUpsert = new Lease
        {
            Id = leaseId,
            Token = token,
            Expires = expires,
            Acquired = null, // Don't update Acquired when renewing
            Released = null, // Don't update Released when renewing
        };
        var result = await UpsertLease(leaseUpsert, now, null, cancellationToken);

        if (result.IsLeaseAcquired) 
        {
            Log.LeaseRenewed(_logger, result.Lease.LeaseId);
        }
        else
        {
            Log.LeaseNotAcquiredAlreadyHeld(_logger, leaseId);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<LeaseReleaseResult> ReleaseLease(LeaseTicket lease, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var expires = DateTimeOffset.MinValue;
        var leaseId = lease.LeaseId;
        var token = lease.Token;

        var leaseUpsert = new Lease
        {
            Id = leaseId,
            Token = token,
            Expires = expires,
            Acquired = null, // Don't update Acquired when releasing
            Released = now,
        };

        var result = await UpsertLease(leaseUpsert, now, null, cancellationToken);

        var released = result.IsLeaseAcquired;

        if (released)
        {
            Log.LeaseReleased(_logger, leaseId);
        }

        return new LeaseReleaseResult
        {
            IsReleased = released,
            Expires = result.Expires,
            LastAcquiredAt = result.LastAcquiredAt,
            LastReleasedAt = result.LastReleasedAt,
        };
    }

    private async Task<LeaseAcquireResult> UpsertLease(Lease upsert, DateTimeOffset now, Func<LeaseInfo, bool>? filter, CancellationToken cancellationToken)
    {
        var leaseRepo = _leaseRepository;
        return await _retryLeasePipeline.ExecuteAsync(
            callback: static (s, ct) =>
            {
                var (leaseRepo, upsert, now, filter) = s;
                var task = UpsertLeaseInner(leaseRepo, upsert, now, filter, ct);
                return new ValueTask<LeaseAcquireResult>(task);
            },
            state: (leaseRepo, upsert, now, filter),
            cancellationToken: cancellationToken);

        async static Task<LeaseAcquireResult> UpsertLeaseInner(ILeaseRepository leaseRepository, Lease upsert, DateTimeOffset now, Func<LeaseInfo, bool>? filter, CancellationToken cancellationToken)
        {
            var result = await leaseRepository.UpsertLease(upsert, now, filter, cancellationToken);
            return result;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Debug, "Failed to upsert lease due to serialization error, retrying...")]
        public static partial void FailedToUpsertLeaseDueToSerializationError(ILogger logger);

        [LoggerMessage(1, LogLevel.Debug, "Lease {LeaseId} acquired")]
        public static partial void LeaseAcquired(ILogger logger, string leaseId);

        [LoggerMessage(2, LogLevel.Debug, "Lease {LeaseId} renewed")]
        public static partial void LeaseRenewed(ILogger logger, string leaseId);

        [LoggerMessage(3, LogLevel.Debug, "Lease {LeaseId} released")]
        public static partial void LeaseReleased(ILogger logger, string leaseId);

        [LoggerMessage(4, LogLevel.Debug, "Lease {LeaseId} was not acquired as it is already held")]
        public static partial void LeaseNotAcquiredAlreadyHeld(ILogger logger, string leaseId);
    }
}
