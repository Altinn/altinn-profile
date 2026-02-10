using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class AddressVerificationControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ProfileWebApplicationFactory<Program> _factory;

        public AddressVerificationControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.AddressVerificationRepositoryMock.Reset();
        }

        [Fact]
        public async Task VerifyAddress_WhenCodeIsCorrect_ReturnsSuccess()
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Address = "address",
                AddressType = AddressType.Email,
                VerificationCode = "verificationCode"
            };

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.TryVerifyAddressAsync(It.IsAny<int>(), It.IsAny<AddressType>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify-address")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task VerifyAddress_WhenCodeIsWrong_ReturnsUnprocessableEntity()
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Address = "address",
                AddressType = AddressType.Email,
                VerificationCode = "verificationCode"
            };

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.TryVerifyAddressAsync(It.IsAny<int>(), It.IsAny<AddressType>(), It.IsAny<string>(), It.IsAny<Func<string, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify-address")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, int userId, string requestUri)
        {
            HttpRequestMessage httpRequestMessage = new(method, requestUri);
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }

        private static HttpRequestMessage AddAuthHeadersToRequest(HttpRequestMessage httpRequestMessage, int userId)
        {
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }
    }
}
