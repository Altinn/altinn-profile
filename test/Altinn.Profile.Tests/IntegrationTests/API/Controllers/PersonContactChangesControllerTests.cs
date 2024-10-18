using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Integrations.Services;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class PersonContactChangesControllerTests
{
    private readonly Mock<IPersonService> _personServiceMock;
    private readonly Mock<ILogger<PersonContactChangesController>> _loggerMock;
    private readonly WebApplicationFactorySetup<PersonContactChangesController> _webApplicationFactorySetup;

    public PersonContactChangesControllerTests()
    {
        _personServiceMock = new Mock<IPersonService>();
        _loggerMock = new Mock<ILogger<PersonContactChangesController>>();
        _webApplicationFactorySetup = new WebApplicationFactorySetup<PersonContactChangesController>(new WebApplicationFactory<PersonContactChangesController>());
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PersonContactChangesController(null, _personServiceMock.Object));
    }

    [Fact]
    public void Constructor_WithNullPersonService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PersonContactChangesController(_loggerMock.Object, null));
    }

    [Fact]
    public async Task SyncChanges_WhenCalled_ReturnsOk()
    {
        // Arrange
        var client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = CreatePostRequest("/profile/api/v1/person/changes/syncchanges");

        // Act
        var response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static HttpRequestMessage CreatePostRequest(string requestUri)
    {
        int userId = 2516356;
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri);
        string token = PrincipalUtil.GetToken(userId);
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpRequestMessage;
    }
}
