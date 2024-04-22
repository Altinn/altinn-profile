using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<ErrorController>>
    {
        private readonly WebApplicationFactorySetup<ErrorController> _webApplicationFactorySetup;

        public ErrorHandlingTests(WebApplicationFactory<ErrorController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<ErrorController>(factory);
        }

        [Fact]
        public async Task GetError_ReturnsInternalServerError()
        {
            // Arrange
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/profile/api/v1/error");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            ProblemDetails problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.StartsWith("An error occurred", problemDetails.Title);
        }
    }
}
