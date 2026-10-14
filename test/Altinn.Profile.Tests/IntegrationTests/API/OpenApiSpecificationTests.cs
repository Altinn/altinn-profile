#nullable enable

using System.Net;
using System.Net.Http;
using System.Text.Json;
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
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOpenApiSpecification_DeleteNotificationSettings_DocumentsOkWithoutBody()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/swagger/v1/swagger.json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using JsonDocument document = JsonDocument.Parse(body);

        JsonElement successResponse = document.RootElement
            .GetProperty("paths")
            .GetProperty("/profile/api/v1/users/current/notificationsettings/parties/{partyUuid}")
            .GetProperty("delete")
            .GetProperty("responses")
            .GetProperty("200");

        Assert.False(successResponse.TryGetProperty("content", out _));
    }
}
