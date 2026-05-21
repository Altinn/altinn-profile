using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Jobs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Jobs;

public class SyncJobTests
{
    [Fact]
    public async Task RunAsync_WhenCalled_InvokesContactRegisterSync()
    {
        // Arrange
        var updateJob = new Mock<IContactRegisterUpdateJob>();
        updateJob
            .Setup(j => j.SyncContactInformationAsync())
            .Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<KrrSyncJob>>();
        var target = new KrrSyncJob(updateJob.Object, logger.Object);

        // Act
        await ((IJob)target).RunAsync(TestContext.Current.CancellationToken);

        // Assert
        updateJob.Verify(j => j.SyncContactInformationAsync(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenSyncThrows_LogsErrorAndDoesNotThrow()
    {
        // Arrange
        var expectedException = new InvalidOperationException("sync failed");

        var updateJob = new Mock<IContactRegisterUpdateJob>();
        updateJob
            .Setup(j => j.SyncContactInformationAsync())
            .ThrowsAsync(expectedException);

        var logger = new Mock<ILogger<KrrSyncJob>>();
        var target = new KrrSyncJob(updateJob.Object, logger.Object);

        // Act
        await ((IJob)target).RunAsync(TestContext.Current.CancellationToken);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => value.ToString()!.Contains("An error occurred during the background synchronization.", StringComparison.Ordinal)),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

public class OrgSyncJobTests
{
    [Fact]
    public async Task RunAsync_WhenCalled_InvokesOrganizationSync()
    {
        // Arrange
        var updateJob = new Mock<IOrganizationNotificationAddressSyncJob>();
        updateJob
            .Setup(j => j.SyncNotificationAddressesAsync())
            .Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<OrgSyncJob>>();
        var target = new OrgSyncJob(updateJob.Object, logger.Object);

        // Act
        await ((IJob)target).RunAsync(TestContext.Current.CancellationToken);

        // Assert
        updateJob.Verify(j => j.SyncNotificationAddressesAsync(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenSyncThrows_LogsErrorAndDoesNotThrow()
    {
        // Arrange
        var expectedException = new InvalidOperationException("sync failed");

        var updateJob = new Mock<IOrganizationNotificationAddressSyncJob>();
        updateJob
            .Setup(j => j.SyncNotificationAddressesAsync())
            .ThrowsAsync(expectedException);

        var logger = new Mock<ILogger<OrgSyncJob>>();
        var target = new OrgSyncJob(updateJob.Object, logger.Object);

        // Act
        await ((IJob)target).RunAsync(TestContext.Current.CancellationToken);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => value.ToString()!.Contains("An error occurred during the background synchronization.", StringComparison.Ordinal)),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

public class JobsServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSyncJobs_WhenEnabled_RegistersJobsWithExpectedLeaseNamesAndIntervals()
    {
        // Arrange
        var services = new ServiceCollection();

        var settings = new Dictionary<string, string>
        {
            ["JobSettings:KrrSyncEnabled"] = "true",
            ["JobSettings:KrrSyncWaitTimeInMinutes"] = "7",
            ["JobSettings:OrgSyncEnabled"] = "true",
            ["JobSettings:OrgSyncWaitTimeInMinutes"] = "11"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var extensionType = typeof(Program).Assembly.GetType("Altinn.Profile.Extensions.JobsServiceCollectionExtensions", throwOnError: true);
        MethodInfo addSyncJobs = extensionType?.GetMethod("AddSyncJobs", BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        addSyncJobs?.Invoke(null, new object[] { services, configuration });

        var registrations = services
            .Where(descriptor => descriptor.ServiceType == typeof(JobRegistration))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<JobRegistration>()
            .ToList();

        // Assert
        Assert.Equal(2, registrations.Count);
        Assert.Contains(registrations, registration => registration.LeaseName == "krr-sync-job" && registration.Interval == TimeSpan.FromMinutes(7));
        Assert.Contains(registrations, registration => registration.LeaseName == "org-sync-job" && registration.Interval == TimeSpan.FromMinutes(11));
    }
}
