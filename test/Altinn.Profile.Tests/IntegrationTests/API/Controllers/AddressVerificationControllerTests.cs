using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Integrations.AddressVerification;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc;

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
            Assert.Equal(2, verifiedAddressResponse.Count);
            Assert.Equal(verifiedAddresses[0].Address, verifiedAddressResponse[0].Value);
            Assert.Equal(verifiedAddresses[0].AddressType, verifiedAddressResponse[0].Type);
            Assert.Equal(verifiedAddresses[1].Address, verifiedAddressResponse[1].Value);
            Assert.Equal(verifiedAddresses[1].AddressType, verifiedAddressResponse[1].Type);
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

            Assert.NotNull(verifiedAddressResponse);
            Assert.Empty(verifiedAddressResponse);
        }

        [Fact]
        public async Task GetVerifiedAddresses_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"profile/api/v1/users/current/verification/verified-addresses");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetVerifiedAddresses_ReturnsBadRequest_WhenSystemUserToken()
        {
            // Arrange
            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"profile/api/v1/users/current/verification/verified-addresses");
            httpRequestMessage = AddSystemUserAuthHeadersToRequest(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task VerifyAddress_WhenCodeIsCorrect_ReturnsSuccess()
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Value = "Address@example.com",
                Type = AddressType.Email,
                VerificationCode = "123456"
            };

            var hash = VerificationCodeService.HashCode(request.VerificationCode);

            var verificationCode = new VerificationCode
            {
                UserId = userId,
                AddressType = AddressType.Email,
                Address = "address@example.com",
                VerificationCodeHash = hash,
                Expires = DateTime.UtcNow.AddHours(1),
            };

            // Address will be formatted to lowercase, so mock with the formatted version
            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerificationCodeAsync(userId, AddressType.Email, verificationCode.Address, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationCode);

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.CompleteAddressVerificationAsync(It.IsAny<int>(), AddressType.Email, "address@example.com", userId))
                .Returns(Task.CompletedTask);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task VerifyAddress_WhenCodeIsWrong_ReturnsUnprocessableEntity()
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Value = "address@example.com",
                Type = AddressType.Email,
                VerificationCode = "wrongcode"
            };
            var hash = VerificationCodeService.HashCode("123456");

            var verificationCode = new VerificationCode
            {
                UserId = userId,
                AddressType = AddressType.Email,
                Address = "address@example.com",
                VerificationCodeHash = hash,
                Expires = DateTime.UtcNow.AddHours(1),
            };

            // Address will be formatted to lowercase, so mock with the formatted version
            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerificationCodeAsync(userId, AddressType.Email, "address@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationCode);

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.IncrementFailedAttemptsAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var responseObject = JsonSerializer.Deserialize<ProblemDetails>(responseContent, _serializerOptionsCamelCase);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Detail);
            Assert.NotNull(responseObject.Title);
        }

        [Fact]
        public async Task VerifyAddress_WhenCodeIsExpired_ReturnsUnprocessableEntity()
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Value = "address@EXAMPLE.com",
                Type = AddressType.Email,
                VerificationCode = "123456"
            };
            var hash = VerificationCodeService.HashCode(request.VerificationCode);

            var verificationCode = new VerificationCode
            {
                UserId = userId,
                AddressType = AddressType.Email,
                Address = "address@example.com",
                VerificationCodeHash = hash,
                Expires = DateTime.UtcNow.AddHours(-1),
            };

            // Address will be formatted to lowercase, so mock with the formatted version
            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerificationCodeAsync(userId, AddressType.Email, "address@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationCode);

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.IncrementFailedAttemptsAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var responseObject = JsonSerializer.Deserialize<ProblemDetails>(responseContent, _serializerOptionsCamelCase);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Detail);
            Assert.NotNull(responseObject.Title);
        }

        [Theory]
        [InlineData(null, AddressType.Email, "123456")]
        [InlineData("Address@email.com", null, "123456")]
        [InlineData("+4798765432", AddressType.Sms, null)]
        public async Task VerifyAddress_WhenIncompleteRequest_ReturnsBadRequest(string address, AddressType? addressType, string code)
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Value = address,
                Type = addressType,
                VerificationCode = code
            };

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task VerifyAddress_WhenUserHasNoStoredCode_ReturnsUnprocessableEntity()
        {
            // Arrange
            const int userId = 2516356;
            var request = new AddressVerificationRequest
            {
                Value = "address@EXAMPLE.com",
                Type = AddressType.Email,
                VerificationCode = "123456"
            };

            // Address will be formatted to lowercase, so mock with the formatted version
            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerificationCodeAsync(userId, AddressType.Email, "address@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((VerificationCode)null);

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.IncrementFailedAttemptsAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var responseObject = JsonSerializer.Deserialize<ProblemDetails>(responseContent, _serializerOptionsCamelCase);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Detail);
            Assert.NotNull(responseObject.Title);
        }

        [Fact]
        public async Task VerifyAddress_WhenCodeIsWrongTooManyTimes_ReturnsTooManyRequests()
        {
            // Arrange
            const int userId = 9999999;
            var request = new AddressVerificationRequest
            {
                Value = "address@example.com",
                Type = AddressType.Email,
                VerificationCode = "wrongcode"
            };
            var hash = VerificationCodeService.HashCode("123456");

            var verificationCode = new VerificationCode
            {
                UserId = userId,
                AddressType = AddressType.Email,
                Address = "address@example.com",
                VerificationCodeHash = hash,
                Expires = DateTime.UtcNow.AddHours(1),
            };

            // Address will be formatted to lowercase, so mock with the formatted version
            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.GetVerificationCodeAsync(userId, AddressType.Email, "address@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationCode);

            _factory.AddressVerificationRepositoryMock.Setup(repo => repo.IncrementFailedAttemptsAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            HttpClient client = _factory.CreateClient();

            // Act
            for (int i = 0; i < 10; i++)
            {
                HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
                {
                    Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
                };
                httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

                var loopResponse = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
                Assert.NotEqual(HttpStatusCode.TooManyRequests, loopResponse.StatusCode);
            }

            HttpRequestMessage lastHttpRequestMessage = new(HttpMethod.Post, $"profile/api/v1/users/current/verification/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(request, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            lastHttpRequestMessage = AddAuthHeadersToRequest(lastHttpRequestMessage, userId);

            HttpResponseMessage response = await client.SendAsync(lastHttpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        private static HttpRequestMessage AddAuthHeadersToRequest(HttpRequestMessage httpRequestMessage, int userId)
        {
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }

        // Creates a request with a system user token (no userId claim)
        private static HttpRequestMessage AddSystemUserAuthHeadersToRequest(HttpRequestMessage httpRequestMessage)
        {
            string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }
    }
}
