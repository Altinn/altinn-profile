using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class TriggerControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    public TriggerControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.ContactRegisterServiceMock.Reset();
        _factory.OrganizationNotificationAddressSyncClientMock.Reset();
    }

    [Fact]
    public async Task SyncChanges_WhenCalled_ReturnsOk()
    {
        // Arrange
        ContactRegisterChangesLog changeLog = new ContactRegisterChangesLog 
        { 
            ContactPreferencesSnapshots = ImmutableList.Create<PersonContactPreferencesSnapshot>() 
        };
        _factory.ContactRegisterServiceMock.Setup(
            c => c.GetContactDetailsChangesAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(changeLog);

        var client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = CreateGetRequest("/profile/api/v1/trigger/syncpersonchanges");

        // Act
        var response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SyncOrgChanges_WhenCalled_ReturnsOk()
    {
        // Arrange
        var changes = new NotificationAddressChangesLog
        {
            OrganizationNotificationAddressList = new List<Entry>(),
        };
        _factory.OrganizationNotificationAddressSyncClientMock.Setup(
            c => c.GetAddressChangesAsync(It.IsAny<string>())).ReturnsAsync(changes);

        var client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = CreateGetRequest("/profile/api/v1/trigger/syncorgchanges");

        // Act
        var response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SyncOrgChanges_WhenSomethingsGoesWrong_ReturnsInternalServerError()
    {
        // Arrange
        _factory.OrganizationNotificationAddressSyncClientMock.Setup(
            c => c.GetAddressChangesAsync(It.IsAny<string>())).ThrowsAsync(new OrganizationNotificationAddressChangesException("Something went wrong"));
        var client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = CreateGetRequest("/profile/api/v1/trigger/syncorgchanges");

        // Act
        var response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private static HttpRequestMessage CreateGetRequest(string requestUri)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, requestUri);

        return httpRequestMessage;
    }
}
