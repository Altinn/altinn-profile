#nullable enable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests;

public class HealthCheckTests(ProfileWebApplicationFactory<Program> factory) 
    : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/health");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
