#nullable enable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API;

public class OpenApiSpecificationTests(ProfileWebApplicationFactory<Program> factory) 
    : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetOpenApiSpecification_ReturnsOk()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/swagger");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
