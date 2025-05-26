using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;

using OpenTelemetry;
using OpenTelemetry.Metrics;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests;

public class TelemetryTests(WebApplicationFactory<Program> factory) 
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactorySetup<Program> _webApplicationFactorySetup = new(factory);

    private MeterProvider _meterProvider;

    [Fact]
    public async Task Init_Initializing_meters_Meters_are_initialized()
    {
        var metricItems = new List<Metric>();

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("platform-profile")
            .AddInMemoryExporter(metricItems)
            .Build();

        using (var client = _webApplicationFactorySetup.GetTestServerClient())
        {
            // We need to call any endpoint that includes some telemetry.
            using var response = await client.GetAsync(new Uri("/profile/api/v1/trigger/syncpersonchanges", UriKind.Relative));
            response.EnsureSuccessStatusCode();
        }

        // We need to let End callback execute as it is executed AFTER response was returned.
        // In unit tests environment there may be a lot of parallel unit tests executed, so
        // giving some breezing room for the End callback to complete
        await Task.Delay(TimeSpan.FromSeconds(1));

        _meterProvider.Dispose();

        var addedMetrics = metricItems
            .Where(item => item.Name == "profile.contactregistry.person.added")
            .ToArray();

        var updatedMetrics = metricItems
            .Where(item => item.Name == "profile.contactregistry.person.updated")
            .ToArray();

        Assert.Single(addedMetrics);
        Assert.Single(updatedMetrics);
    }

    public void Dispose()
    {
        _meterProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
