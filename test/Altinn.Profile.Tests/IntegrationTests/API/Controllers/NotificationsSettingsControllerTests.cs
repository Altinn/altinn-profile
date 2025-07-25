using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;

using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class NotificationsSettingsControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactorySetup<Program> _webApplicationFactorySetup = new WebApplicationFactorySetup<Program>(factory);

        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private void SetupSblMock()
        {
            DelegatingHandlerStub messageHandler = new((request, token) =>
            {
                UserProfile userProfile = new UserProfile { ProfileSettingPreference = new ProfileSettingPreference { Language = "nb" } };
                return Task.FromResult(new HttpResponseMessage() { Content = JsonContent.Create(userProfile) });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;
        }

        private static void SetupAuthHandler(WebApplicationFactorySetup<Program> _webApplicationFactorySetup, Guid partyGuid, int UserId, bool access = true)
        {
            _webApplicationFactorySetup.RegisterClientMock
                .Setup(x => x.GetPartyId(partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((int)partyGuid.GetHashCode()); // Simulate party ID retrieval
            _webApplicationFactorySetup.AuthorizationClientMock
                .Setup(x => x.ValidateSelectedParty(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);
        }

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

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPartyContactInfo);

            SetupSblMock();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
                .Setup(x => x.GetNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserPartyContactInfo)null);
            SetupSblMock();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
        public async Task PutNotificationAddress_WhenUserIDoesNotHaveAccess_ReturnForbidden()
        {
            // Arrange
            var partyGuid = Guid.NewGuid();
            var userId = 2516356;

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "+4798765432",
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, userId, false);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
                ResourceIncludeList = ["example"]
            };
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(content, _serializerOptionsCamelCase);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Equal(2, actual.Errors.Count);
            Assert.NotNull(actual.Errors["EmailAddress"]);
            Assert.True(actual.Errors.TryGetValue("EmailAddress", out var message));
            Assert.Contains("The field EmailAddress must match the regular expression", message[0]);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenContactInfoIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                PhoneNumber = string.Empty,
                ResourceIncludeList = ["urn:altinn:resource:example"]
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(content, _serializerOptionsCamelCase);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Equal(2, actual.Errors.Count);
            Assert.NotNull(actual.Errors["EmailAddress"]);
            Assert.NotNull(actual.Errors["PhoneNumber"]);
            Assert.True(actual.Errors.TryGetValue("EmailAddress", out var message));
            Assert.Contains("The notification setting for a party must include either EmailAddress, PhoneNumber, or both.", message[0]);
        }

        [Theory]
        [InlineData("example")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")] // whitespace
        [InlineData("urn:altinn:resource")]
        [InlineData("urn:altinn:resource:abc")] // Too short resource ID
        [InlineData("urn:altinn:resource:some*resource")] // Contains invalid char
        public async Task PutNotificationAddress_WhenResourcesIsInvalid_ReturnsBadRequest(string resource)
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "+4798765432",
                ResourceIncludeList = [resource]
            };
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(content, _serializerOptionsCamelCase);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Single(actual.Errors);
            Assert.NotNull(actual.Errors["ResourceIncludeList"]);
            Assert.True(actual.Errors.TryGetValue("ResourceIncludeList", out var message));
            Assert.Contains("ResourceIncludeList must contain valid URN values of the format 'urn:altinn:resource:{resourceId}' where resourceId has 4 or more characters of lowercase letter, number, underscore or hyphen", message[0]);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenResourcesContainsDuplicates_ReturnsBadRequest()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "+4798765432",
                ResourceIncludeList = ["urn:altinn:resource:example", "urn:altinn:resource:example"]
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(content, _serializerOptionsCamelCase);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Single(actual.Errors);
            Assert.NotNull(actual.Errors["ResourceIncludeList"]);
            Assert.True(actual.Errors.TryGetValue("ResourceIncludeList", out var message));
            Assert.Contains("ResourceIncludeList cannot contain duplicates", message[0]);
        }

        [Theory]
        [InlineData("urn:altinn:resource:example")]
        [InlineData("urn:altinn:resource:app_other_vale")]
        [InlineData("urn:altinn:resource:ttd-resource-1")]

        public async Task PutNotificationAddress_WhenContactInfoIsNew_ReturnsCreated(string resource)
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new ProfessionalNotificationAddressRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "12345678",
                ResourceIncludeList = [resource]
            };

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.AddOrUpdateNotificationAddressAsync(It.IsAny<UserPartyContactInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            SetupSblMock();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

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
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.AddOrUpdateNotificationAddressAsync(It.IsAny<UserPartyContactInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            SetupSblMock();

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

        [Fact]
        public async Task DeleteNotificationAddress_WhenRepositoryReturnsNull_ReturnsNotFound()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.DeleteNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserPartyContactInfo)null);
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Notification addresses not found", responseContent);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenRepositoryReturnsAddress_ReturnsOk()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.DeleteNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPartyContactInfo());
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
