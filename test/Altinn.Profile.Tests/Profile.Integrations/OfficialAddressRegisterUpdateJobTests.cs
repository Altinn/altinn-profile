using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.OfficialAddressRegister;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;
using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class OfficialAddressRegisterUpdateJobTests()
{
    private readonly OfficialAddressRegisterSettings _settings = new() { ChangesLogEndpoint = "https://example.com/changes", ChangesLogPageSize = 10000 };
    private readonly Mock<IOfficialAddressMetadataRepository> _metadataRepository = new();
    private readonly Mock<IOfficialAddressHttpClient> _httpClient = new();

    [Fact]
    public async Task SyncContactInformationAsyncTest_Missing_endpoint_Cause_InvalidOperationException()
    {
        // Arrange
        OfficialAddressRegisterSettings settings = new();
        OfficialAddressRegisterUpdateJob target =
            new(settings, _httpClient.Object, _metadataRepository.Object);

        // Act
        InvalidOperationException actual = null;

        try
        {
            await target.SyncContactInformationAsync();
        }
        catch (InvalidOperationException ioe)
        {
            actual = ioe;
        }

        // Assert
        Assert.NotNull(actual);
    }

    [Fact]
    public async Task SyncContactInformationAsyncTest_Expected_work()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>()))
            .ReturnsAsync(await TestDataLoader.Load<OfficialAddressRegisterChangesLog>("changes_1"))
            .ReturnsAsync(await TestDataLoader.Load<OfficialAddressRegisterChangesLog>("changes_2"));
        /*
        _personRepository.SetupSequence(p => p.SyncPersonContactPreferencesAsync(It.IsAny<OfficialAddressRegisterChangesLog>()))
            .ReturnsAsync(10)
            .ReturnsAsync(5);*/

        OfficialAddressRegisterUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object);

        // Act
        await target.SyncContactInformationAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
    }
}
