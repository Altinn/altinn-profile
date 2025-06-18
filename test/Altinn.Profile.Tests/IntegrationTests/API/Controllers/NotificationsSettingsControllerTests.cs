using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Controllers;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class NotificationsSettingsControllerTests(WebApplicationFactory<NotificationsSettingsController> factory) : IClassFixture<WebApplicationFactory<NotificationsSettingsController>>
    {
        private readonly WebApplicationFactorySetup<NotificationsSettingsController> _webApplicationFactorySetup = new WebApplicationFactorySetup<NotificationsSettingsController>(factory);

        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Fact]
        public async Task GetNotificationAddress_WhenRepositoryReturnsValues_IsOk()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new UserPartyContactInfo
            {
                UserId = UserId,
                PartyUuid = partyGuid,
                EmailAddress = "test@example.com",
                PhoneNumber = "12345678",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "urn:altinn:resource:example" }
                }
            };

            var resource = 

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetNotificationAddress(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPartyContactInfo);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            ProfessionalNotificationAddressResponse notificationAddresses = JsonSerializer.Deserialize<ProfessionalNotificationAddressResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Equal(UserId, notificationAddresses.UserId);
            Assert.Equal(partyGuid, notificationAddresses.PartyUuid);
            Assert.Equal("test@example.com", notificationAddresses.EmailAddress);
            Assert.Equal("12345678", notificationAddresses.PhoneNumber);
            Assert.NotNull(notificationAddresses.ResourceIncludeList);
            Assert.Single(notificationAddresses.ResourceIncludeList);
            Assert.Equal("urn:altinn:resource:example", notificationAddresses.ResourceIncludeList[0]);
        }

        [Fact]
        public async Task GetNotificationAddress_WhenRepositoryReturnsNull_ReturnsNotFound()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetNotificationAddress(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserPartyContactInfo)null);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Notification addresses not found", responseContent);
        }

        [Fact]
        public async Task GetNotificationAddress_WhenNoUserId_ReturnsUnauthorized()
        {
            // Arrange
            var partyGuid = Guid.NewGuid();

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenContactInfoIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@@example.com",
                PhoneNumber = "++",
                ResourceIncludeList = ["urn:altinn:resource:example"]
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenContactInfoIsNew_ReturnsCreated()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "12345678",
                ResourceIncludeList = ["urn:altinn:resource:example"]
            };

            var resource =

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.AddOrUpdateNotificationAddressAsync(It.IsAny<UserPartyContactInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenContactInfoAlreadyExists_ReturnsNoContent()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "12345678",
                ResourceIncludeList = ["urn:altinn:resource:example"]
            };

            var resource =

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.AddOrUpdateNotificationAddressAsync(It.IsAny<UserPartyContactInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, int userId, string requestUri)
        {
            HttpRequestMessage httpRequestMessage = new(method, requestUri);
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);
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
