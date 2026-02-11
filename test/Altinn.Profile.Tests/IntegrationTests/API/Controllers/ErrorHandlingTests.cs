#nullable enable

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class ErrorHandlingTests(ProfileWebApplicationFactory<Program> factory) 
    : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetError_ReturnsInternalServerError()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/profile/api/v1/error");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.StartsWith("An error occurred", problemDetails?.Title);
    }
}
