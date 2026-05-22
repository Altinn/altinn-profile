using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Jobs;

using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry;
using OpenTelemetry.Metrics;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests;

public class TelemetryTests
{
    public class RegistryMetrics(ProfileWebApplicationFactory<Program> factory)
        : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task SyncPersonChanges_WhenCalled_CreatesContactRegistryMetrics()
        {
            var metricItems = new List<Metric>();

            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("platform-profile")
                .AddInMemoryExporter(metricItems)
                .Build();

            using var scope = _factory.Services.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<KrrSyncJob>();

            await ((IJob)job).RunAsync(TestContext.Current.CancellationToken);

            meterProvider.ForceFlush();

            var addedMetrics = metricItems
                .Where(item => item.Name == "profile.contactregistry.person.added")
                .ToArray();

            var updatedMetrics = metricItems
                .Where(item => item.Name == "profile.contactregistry.person.updated")
                .ToArray();

            Assert.Single(addedMetrics);
            Assert.Single(updatedMetrics);
        }
    }

    public class NotificationAddressMetrics(ProfileWebApplicationFactory<Program> factory)
        : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task SyncOrgChanges_WhenCalled_CreatesOrganizationNotificationAddressMetrics()
        {
            var metricItems = new List<Metric>();

            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("platform-profile")
                .AddInMemoryExporter(metricItems)
                .Build();

            using var scope = _factory.Services.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<OrgSyncJob>();

            await ((IJob)job).RunAsync(TestContext.Current.CancellationToken);

            meterProvider.ForceFlush();

            var addedOrgMetrics = metricItems
                .Where(item => item.Name == "profile.organizationnotificationaddress.organization.added")
                .ToArray();

            var addedMetrics = metricItems
                .Where(item => item.Name == "profile.organizationnotificationaddress.address.added")
                .ToArray();

            var updatedMetrics = metricItems
                .Where(item => item.Name == "profile.organizationnotificationaddress.address.updated")
                .ToArray();

            var deletedMetrics = metricItems
                .Where(item => item.Name == "profile.organizationnotificationaddress.address.deleted")
                .ToArray();

            Assert.Single(addedOrgMetrics);
            Assert.Single(addedMetrics);
            Assert.Single(updatedMetrics);
            Assert.Single(deletedMetrics);
        }
    }
}
