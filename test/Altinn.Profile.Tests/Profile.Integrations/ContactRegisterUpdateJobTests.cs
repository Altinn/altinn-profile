using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;
using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class ContactRegisterUpdateJobTests()
{
    private readonly ContactRegisterSettings _settings = new() { ChangesLogEndpoint = "https://example.com/changes" };
    private readonly Mock<IMetadataRepository> _metadataRepository = new();
    private readonly Mock<IContactRegisterHttpClient> _httpClient = new();
    private readonly Mock<IPersonUpdater> _personRepository = new();

    [Fact]
    public async Task SyncContactInformationAsyncTest_Missing_endpoint_Cause_InvalidOperationException()
    {
        // Arrange
        ContactRegisterSettings settings = new();
        ContactRegisterUpdateJob target =
            new(settings, _httpClient.Object, _metadataRepository.Object, _personRepository.Object);

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
        _metadataRepository.SetupSequence(m => m.GetLatestChangeNumberAsync())
            .ReturnsAsync(23)
            .ReturnsAsync(33);
        _metadataRepository.Setup(m => m.UpdateLatestChangeNumberAsync(It.Is<long>(i => i == 33)))
            .ReturnsAsync(1);
        _metadataRepository.Setup(m => m.UpdateLatestChangeNumberAsync(It.Is<long>(i => i == 38)))
            .ReturnsAsync(1);

        _httpClient.Setup(h => h.GetContactDetailsChangesAsync(It.IsAny<string>(), It.Is<long>(i => i == 23)))
            .ReturnsAsync(await TestDataLoader.Load<ContactRegisterChangesLog>("changes_1"));
        _httpClient.Setup(h => h.GetContactDetailsChangesAsync(It.IsAny<string>(), It.Is<long>(i => i == 33)))
            .ReturnsAsync(await TestDataLoader.Load<ContactRegisterChangesLog>("changes_2"));

        _personRepository.SetupSequence(p => p.SyncPersonContactPreferencesAsync(It.IsAny<ContactRegisterChangesLog>()))
            .ReturnsAsync(10)
            .ReturnsAsync(5);

        ContactRegisterUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _personRepository.Object);

        // Act
        await target.SyncContactInformationAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _personRepository.VerifyAll();
    }
}
