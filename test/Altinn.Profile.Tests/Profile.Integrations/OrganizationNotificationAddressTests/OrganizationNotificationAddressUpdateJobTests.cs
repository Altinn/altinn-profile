using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests;

public class OrganizationNotificationAddressUpdateJobTests()
{
    private readonly Mock<IRegistrySyncMetadataRepository> _metadataRepository = new();
    private readonly Mock<IOrganizationNotificationAddressUpdater> _organizationNotificationAddressUpdater = new();
    private readonly Mock<IOrganizationNotificationAddressSyncClient> _httpClient = new();
    private readonly Mock<ILogger<OrganizationNotificationAddressUpdateJob>> _logger = new();

    [Fact]
    public async Task SyncNotificationAddressesAsync_IfNoEntries_DoNothing()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_0_faulty"));

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenClientReturnsNoChangesLog_DoesNotAdvanceWatermark()
    {
        // Arrange
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationAddressChangesLog)null);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _httpClient.Verify(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _metadataRepository.Verify(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _organizationNotificationAddressUpdater.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_IfNoChanges_DoNothing()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_0"));

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsyncTest_Success()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1"))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.SetupSequence(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2)
            .ReturnsAsync(4);

        _metadataRepository.Setup(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()));

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsyncTest_WhenNoLatestSyncDate_GetsWithoutSinceParam()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync((DateTime?)null);
        _httpClient.Setup(h => h.GetInitialUrl(null))
            .Returns("https://test.no/changes?pageSize=100");

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.Is<string>(s => !s.Contains("since")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.SetupSequence(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        _metadataRepository.Setup(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()));

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsyncTest_WhenNullableValues_Success()
    {
        // Arrange
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_3"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenUpdaterThrows_DoesNotAdvanceWatermark()
    {
        // Arrange
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OrganizationNotificationAddressChangesException("boom"));

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act and Assert
        await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(() => target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken));

        // A page that could not be persisted must be retried from the same point on the next run.
        _metadataRepository.Verify(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// KoFuVi has confirmed that the feed is sorted ascending on <c>updated</c> and that <c>since</c> is
    /// exclusive (it returns entries where <c>updated</c> &gt; <c>since</c>). A page therefore has to advance the
    /// watermark even when it caused no database writes, otherwise the very same page is requested again on the
    /// next run, produces no writes again, and the synchronization is permanently stuck.
    /// </summary>
    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenPageCausedNoDatabaseWrites_StillAdvancesWatermark()
    {
        // Arrange - every entry in this page is a no-op because the identifier type is not an organization number
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_6"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 1, 16, 9, 7, 11, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenPageCausedNoDatabaseWrites_ContinuesToNextPage()
    {
        // Arrange
        const string InitialUrl = "https://kof.test/changes?pageSize=100";
        const string NextPageUrl = "http://someurl.no/page2";

        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        _httpClient.Setup(h => h.GetInitialUrl(It.IsAny<DateTime?>())).Returns(InitialUrl);

        _httpClient.Setup(h => h.GetAddressChangesAsync(InitialUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_noop_with_next_page"));
        _httpClient.Setup(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.SetupSequence(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0)
            .ReturnsAsync(4);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert - a page without database writes must not abort the run
        _httpClient.Verify(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()), Times.Once);
        _organizationNotificationAddressUpdater.Verify(
            p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_AdvancesWatermarkToUpdatedOfLastEntryInPage()
    {
        // Arrange
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert - the feed is sorted ascending, so the last entry of the page is the new watermark
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 1, 16, 9, 7, 11, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Once); // Verify that the method is called exacly once with the given argument
        _metadataRepository.Verify(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once); // Verify that the method is called exactly once in total with _any_ input - i.e., the method is not called with any other input than what is verified above
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenPageHasNextPage_RequestsThatExactUrl()
    {
        // Arrange
        const string InitialUrl = "https://kof.test/changes?pageSize=100";
        const string NextPageUrl = "http://someurl.no/next";

        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        _httpClient.Setup(h => h.GetInitialUrl(It.IsAny<DateTime?>())).Returns(InitialUrl);

        _httpClient.Setup(h => h.GetAddressChangesAsync(InitialUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1"));
        _httpClient.Setup(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert - pagination must follow the link from the feed, not re-request the initial url
        _httpClient.Verify(h => h.GetAddressChangesAsync(InitialUrl, It.IsAny<CancellationToken>()), Times.Once);
        _httpClient.Verify(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()), Times.Once);
        _httpClient.Verify(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// Entries can share an updated timestamp, and such a group can be split across a page boundary. Because
    /// "since" is exclusive, committing the last timestamp of a page that has more pages behind it would make an
    /// interrupted run resume past the group and never read the rest of it.
    /// </summary>
    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenPageEndsWithTiedTimestampsAndMorePagesFollow_DoesNotCommitPastTheGroup()
    {
        // Arrange
        const string InitialUrl = "https://kof.test/changes?pageSize=100";
        const string NextPageUrl = "http://someurl.no/tiedpage2";

        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        _httpClient.Setup(h => h.GetInitialUrl(It.IsAny<DateTime?>())).Returns(InitialUrl);

        _httpClient.Setup(h => h.GetAddressChangesAsync(InitialUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_tied_tail_with_next_page"));
        _httpClient.Setup(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_tied_tail_continued"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert - the 09:00 group continues on the next page, so the first page only commits 08:00 and the
        // group is never committed on its own. The final page then commits its last entry.
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 3, 4, 8, 0, 0, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Once);
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 3, 4, 9, 0, 0, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Never);
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 3, 4, 10, 0, 0, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Once);
        _metadataRepository.Verify(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenAllEntriesInPageShareTimestampAndMorePagesFollow_DoesNotAdvanceWatermark()
    {
        // Arrange
        const string InitialUrl = "https://kof.test/changes?pageSize=100";
        const string NextPageUrl = "http://someurl.no/tiedpage2";

        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        _httpClient.Setup(h => h.GetInitialUrl(It.IsAny<DateTime?>())).Returns(InitialUrl);

        _httpClient.Setup(h => h.GetAddressChangesAsync(InitialUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_all_tied_with_next_page"));
        _httpClient.Setup(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_all_tied_continued"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert - no part of the first page is known to be complete, so it is not committed. The 10:00 group
        // continues on the next page, and only that final page moves the watermark, past the group.
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 3, 5, 10, 0, 0, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Never);
        _metadataRepository.Verify(
            m => m.UpdateLatestChangeTimestampAsync(new DateTime(2025, 3, 5, 11, 0, 0, DateTimeKind.Utc), It.IsAny<CancellationToken>()),
            Times.Once);
        _metadataRepository.Verify(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _httpClient.Verify(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// The token passed to the job is the lease token. If the lease is lost the run has to stop, otherwise
    /// another instance can acquire the lease and process the same feed at the same time.
    /// </summary>
    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenCancelled_StopsBeforeFetchingTheNextPage()
    {
        // Arrange
        const string InitialUrl = "https://kof.test/changes?pageSize=100";
        const string NextPageUrl = "http://someurl.no/next";

        using var cts = new CancellationTokenSource();

        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        _httpClient.Setup(h => h.GetInitialUrl(It.IsAny<DateTime?>())).Returns(InitialUrl);

        _httpClient.Setup(h => h.GetAddressChangesAsync(InitialUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1"));

        // Cancel while the first page is being written, as a lost lease would
        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2)
            .Callback(() => cts.Cancel());

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act and Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => target.SyncNotificationAddressesAsync(cts.Token));

        _httpClient.Verify(h => h.GetAddressChangesAsync(NextPageUrl, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenPageIsEmpty_DoesNotAdvanceWatermark()
    {
        // Arrange
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_0"));

        OrganizationNotificationAddressUpdateJob target =
            new(_httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync(TestContext.Current.CancellationToken);

        // Assert - an empty page means we are caught up; there is nothing to move the watermark to
        _metadataRepository.Verify(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _organizationNotificationAddressUpdater.VerifyNoOtherCalls();
    }
}
