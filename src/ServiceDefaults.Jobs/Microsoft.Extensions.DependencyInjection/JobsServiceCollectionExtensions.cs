using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Register.Jobs;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class JobsServiceCollectionExtensions
{
    /// <summary>
    /// Adds a recurring job to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The job type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">A configuration delegate.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddRecurringJob<T>(this IServiceCollection services, Action<IJobSettings> configure)
        where T : class, IJob
        => services.AddRecurringJob<T>(serviceKey: null, configure);

    /// <summary>
    /// Adds a recurring job to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The job type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="serviceKey">A key to use when resolving the job from the service provider.</param>
    /// <param name="configure">A configuration delegate.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddRecurringJob<T>(this IServiceCollection services, object? serviceKey, Action<IJobSettings> configure)
        where T : class, IJob
    {
        Guard.IsNotNull(services);
        Guard.IsNotNull(configure);

        var settings = new JobSettings();
        configure(settings);

        services.TryAddKeyedScoped<T>(serviceKey);
        return services.AddRecurringJob<T>(settings, serviceKey);
    }

    private static IServiceCollection AddRecurringJob<T>(this IServiceCollection services, JobSettings settings, object? serviceKey)
        where T : IJob
    {
        Guard.IsNotNull(services);
        if (settings.Interval <= TimeSpan.Zero && settings.RunAt == JobHostLifecycles.None)
        {
            ThrowHelper.ThrowArgumentException(nameof(settings), "Interval must be greater than zero or RunAt must be set.");
        }

        var registration = new JobRegistration<T>(settings.LeaseName, settings.Interval, settings.RunAt, settings.Tags, settings.Enabled, settings.WaitForReady, serviceKey);
        return services.AddRecurringJob(registration);
    }

    /// <summary>
    /// Adds a recurring job to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="registration">The <see cref="JobRegistration"/>.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddRecurringJob(this IServiceCollection services, JobRegistration registration)
    {
        Guard.IsNotNull(services);
        Guard.IsNotNull(registration);

        services.AddRecurringJobHostedService();

        services.AddSingleton(registration);
        return services;
    }

    /// <summary>
    /// Adds the job-runner as a hosted service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddRecurringJobHostedService(this IServiceCollection services)
    {
        Guard.IsNotNull(services);

        if (services.Contains(Marker.ServiceDescriptor))
        {
            // already registered
            return services;
        }

        services.Add(Marker.ServiceDescriptor);
        services.TryAddSingleton<JobsTelemetry>();
        services.AddHostedService<RecurringJobHostedService>();

        services.AddOpenTelemetry()
            .WithTracing(t => t.AddSource(JobsTelemetry.Name))
            .WithMetrics(t => t.AddMeter(JobsTelemetry.Name));

        return services;
    }

    private sealed class JobRegistration<T>(
        string? leaseName,
        TimeSpan interval,
        JobHostLifecycles runAt,
        IEnumerable<string> tags,
        Func<IServiceProvider, CancellationToken, ValueTask<bool>>? enabled,
        Func<IServiceProvider, CancellationToken, ValueTask>? waitForReady,
        object? serviceKey)
        : JobRegistration(leaseName, interval, runAt, tags, enabled, waitForReady)
        where T : IJob
    {
        public override IJob Create(IServiceProvider services)
            => services.GetRequiredKeyedService<T>(serviceKey);
    }

    private sealed class JobSettings
        : IJobSettings
    {
        /// <inheritdoc/>
        public string? LeaseName { get; set; } = null;

        /// <inheritdoc/>
        public TimeSpan Interval { get; set; } = TimeSpan.Zero;

        /// <inheritdoc/>
        public JobHostLifecycles RunAt { get; set; } = JobHostLifecycles.None;

        /// <inheritdoc/>
        public ISet<string> Tags { get; } = new HashSet<string>(StringComparer.Ordinal);

        /// <inheritdoc/>
        public Func<IServiceProvider, CancellationToken, ValueTask<bool>>? Enabled { get; set; } = null;

        /// <inheritdoc/>
        public Func<IServiceProvider, CancellationToken, ValueTask>? WaitForReady { get; set; } = null;
    }

    private sealed class Marker
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Marker, Marker>();
    }
}
