﻿using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class TriggerControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<ILogger<TriggerController>> _loggerMock;
    private readonly WebApplicationFactorySetup<Program> _webApplicationFactorySetup;

    public TriggerControllerTests(WebApplicationFactory<Program> factory)
    {
        _loggerMock = new Mock<ILogger<TriggerController>>();
        _webApplicationFactorySetup = new WebApplicationFactorySetup<Program>(factory);
    }

    [Fact]
    public async Task SyncChanges_WhenCalled_ReturnsOk()
    {
        // Arrange
        ContactRegisterChangesLog changeLog = new ContactRegisterChangesLog 
        { 
            ContactPreferencesSnapshots = ImmutableList.Create<PersonContactPreferencesSnapshot>() 
        };
        _webApplicationFactorySetup.ContactRegisterServiceMock.Setup(
            c => c.GetContactDetailsChangesAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(changeLog);

        var client = _webApplicationFactorySetup.GetTestServerClient();

        HttpRequestMessage httpRequestMessage = CreateGetRequest("/profile/api/v1/trigger/syncpersonchanges");

        // Act
        var response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static HttpRequestMessage CreateGetRequest(string requestUri)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, requestUri);

        return httpRequestMessage;
    }
}
