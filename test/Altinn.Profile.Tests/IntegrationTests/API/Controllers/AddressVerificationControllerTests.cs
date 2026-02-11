using System;
using System.Collections.Generic;
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
        public async Task GetVerifiedAddresses_ReturnsOk_WhenMultipleAddresses()
        {
            // Arrange
            var userId = 123;
            var verifiedAddresses = new List<VerifiedAddress>
            {
                new() { UserId = userId, AddressType = AddressType.Email, Address = "test@email.com", VerifiedAt = DateTime.UtcNow, VerificationType = VerificationType.Verified },
                new() { UserId = userId, AddressType = AddressType.Sms, Address = "12345678", VerifiedAt = DateTime.UtcNow, VerificationType = VerificationType.Legacy }
            };

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerifiedAddressesAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verifiedAddresses);

            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"profile/api/v1/users/current/verification/verified-addresses");

            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            List<VerifiedAddressResponse> verifiedAddressResponse = JsonSerializer.Deserialize<List<VerifiedAddressResponse>>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(verifiedAddressResponse);
            Assert.Equal(verifiedAddresses[0].Address, verifiedAddressResponse[0].Value);
            Assert.Equal(verifiedAddresses[0].AddressType, verifiedAddressResponse[0].Type);
            Assert.Equal(verifiedAddresses[0].VerificationType, verifiedAddressResponse[0].VerificationStatus);
            Assert.Equal(verifiedAddresses[1].Address, verifiedAddressResponse[1].Value);
            Assert.Equal(verifiedAddresses[1].AddressType, verifiedAddressResponse[1].Type);
            Assert.Equal(verifiedAddresses[1].VerificationType, verifiedAddressResponse[1].VerificationStatus);
        }

        [Fact]
        public async Task GetVerifiedAddresses_ReturnsOk_WhenNoAddresses()
        {
            // Arrange
            var userId = 123;
            var verifiedAddresses = new List<VerifiedAddress>();

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerifiedAddressesAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verifiedAddresses);

            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"profile/api/v1/users/current/verification/verified-addresses");

            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            List<VerifiedAddressResponse> verifiedAddressResponse = JsonSerializer.Deserialize<List<VerifiedAddressResponse>>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(verifiedAddressResponse);
        }

        private static HttpRequestMessage AddAuthHeadersToRequest(HttpRequestMessage httpRequestMessage, int userId)
        {
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }
    }
}
