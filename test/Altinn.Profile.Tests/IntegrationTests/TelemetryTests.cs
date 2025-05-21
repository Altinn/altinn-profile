using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;

using Moq;

using OpenTelemetry;
using OpenTelemetry.Metrics;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class TelemetryTests(WebApplicationFactory<Program> factory) 
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactorySetup<Program> _webApplicationFactorySetup = new(factory);

    private MeterProvider _meterProvider;

    [Fact]
    public async Task Init_Verify_Creation_of_Meters()
    {
        var metricItems = new List<Metric>();

        this._meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("platform-profile")
            .AddInMemoryExporter(metricItems)
            .Build();

        var changes = new NotificationAddressChangesLog
        {
            OrganizationNotificationAddressList = new List<Entry>(),
        };

        _webApplicationFactorySetup.OrganizationNotificationAddressSyncClientMock.Setup(
            c => c.GetAddressChangesAsync(It.IsAny<string>())).ReturnsAsync(changes);

        using (var client = this._webApplicationFactorySetup.GetTestServerClient())
        {
            try
            {
                // We need to call an endpoint that triggers some telemetry
                using var response = await client.GetAsync(new Uri("/profile/api/v1/trigger/syncorgchanges", UriKind.Relative));
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                // ignore error.
            }
        }

        // We need to let End callback execute as it is executed AFTER response was returned.
        // In unit tests environment there may be a lot of parallel unit tests executed, so
        // giving some breezing room for the End callback to complete
        await Task.Delay(TimeSpan.FromSeconds(1));

        this._meterProvider.Dispose();

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
        this._meterProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
