#nullable enable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests;

public class OtlpExporterTests(ProfileWebApplicationFactory<Program> factory)
    : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetHealth_WithOtlpEndpointConfigured_ReturnsOk()
    {
        // Arrange
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317"));
        HttpClient client = factory.CreateClient();

        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/health");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
