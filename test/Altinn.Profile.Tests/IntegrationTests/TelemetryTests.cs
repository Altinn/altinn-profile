using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

using Altinn.Profile.Core.Telemetry;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests;

public class TelemetryTests : IDisposable
{
    private readonly Telemetry _telemetry = new();
    private readonly MeterListener _meterListener;
    private readonly List<(string InstrumentName, long Value)> _recordedCounters = [];

    public TelemetryTests()
    {
        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == Telemetry.AppName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };

        _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            _recordedCounters.Add((instrument.Name, measurement));
        });

        _meterListener.Start();
    }

    [Fact]
    public void PersonAdded_WhenCalled_EmitsContactRegistryPersonAddedMetric()
    {
        _telemetry.PersonAdded();

        var counter = Assert.Single(_recordedCounters, item => item.InstrumentName == Telemetry.Metrics.CreateName("contactregistry.person.added"));
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public void PersonUpdated_WhenCalled_EmitsContactRegistryPersonUpdatedMetric()
    {
        _telemetry.PersonUpdated();

        var counter = Assert.Single(_recordedCounters, item => item.InstrumentName == Telemetry.Metrics.CreateName("contactregistry.person.updated"));
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public void OrganizationAdded_WhenCalled_EmitsOrganizationNotificationAddressOrganizationAddedMetric()
    {
        _telemetry.OrganizationAdded();

        var counter = Assert.Single(_recordedCounters, item => item.InstrumentName == Telemetry.Metrics.CreateName("organizationnotificationaddress.organization.added"));
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public void AddressAdded_WhenCalled_EmitsOrganizationNotificationAddressAddedMetric()
    {
        _telemetry.AddressAdded();

        var counter = Assert.Single(_recordedCounters, item => item.InstrumentName == Telemetry.Metrics.CreateName("organizationnotificationaddress.address.added"));
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public void AddressUpdated_WhenCalled_EmitsOrganizationNotificationAddressUpdatedMetric()
    {
        _telemetry.AddressUpdated();

        var counter = Assert.Single(_recordedCounters, item => item.InstrumentName == Telemetry.Metrics.CreateName("organizationnotificationaddress.address.updated"));
        Assert.Equal(1, counter.Value);
    }

    [Fact]
    public void AddressDeleted_WhenCalled_EmitsOrganizationNotificationAddressDeletedMetric()
    {
        _telemetry.AddressDeleted();

        var counter = Assert.Single(_recordedCounters, item => item.InstrumentName == Telemetry.Metrics.CreateName("organizationnotificationaddress.address.deleted"));
        Assert.Equal(1, counter.Value);
    }

    public void Dispose()
    {
        _meterListener.Dispose();
        _telemetry.Dispose();
        GC.SuppressFinalize(this);
    }
}
