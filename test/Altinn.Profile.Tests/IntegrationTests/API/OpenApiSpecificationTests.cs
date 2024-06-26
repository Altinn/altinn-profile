using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Profile.Configuration;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API;

public class OpenApiSpecificationTests : IClassFixture<WebApplicationFactory<GeneralSettings>>
{
    private readonly WebApplicationFactorySetup<GeneralSettings> _webApplicationFactorySetup;

    public OpenApiSpecificationTests(WebApplicationFactory<GeneralSettings> factory)
    {
        _webApplicationFactorySetup = new WebApplicationFactorySetup<GeneralSettings>(factory);
    }

    [Fact]
    public async Task GetOpenApiSpecification_ReturnsOk()
    {
        // Arrange
        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/swagger");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
