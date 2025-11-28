using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var client = _factory.CreateClient();

            // We need to call any endpoint that includes some telemetry.
            using var response =
                await client.GetAsync(new Uri("/profile/api/v1/trigger/syncpersonchanges", UriKind.Relative));

            // We need to let End callback execute as it is executed AFTER response was returned.
            // In unit tests environment there may be a lot of parallel unit tests executed, so
            // giving some breezing room for the End callback to complete
            await Task.Delay(TimeSpan.FromSeconds(1));

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

            var client = _factory.CreateClient();

            // We need to call any endpoint that includes some telemetry.
            using var response =
                await client.GetAsync(new Uri("/profile/api/v1/trigger/syncorgchanges", UriKind.Relative));

            // We need to let End callback execute as it is executed AFTER response was returned.
            // In unit tests environment there may be a lot of parallel unit tests executed, so
            // giving some breezing room for the End callback to complete
            await Task.Delay(TimeSpan.FromSeconds(1));

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
