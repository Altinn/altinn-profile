using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class NotificationsSettingsControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public NotificationsSettingsControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.ProfessionalNotificationsRepositoryMock.Reset();
            _factory.RegisterClientMock.Reset();
            _factory.AuthorizationClientMock.Reset();
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
                    new() { ResourceId = "app_example" }
                }
            };

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPartyContactInfo);
            _factory.ProfileSettingsRepositoryMock
                .Setup(x => x.GetProfileSettings(UserId))
                .ReturnsAsync(new ProfileSettings { UserId = UserId, IgnoreUnitProfileDateTime = null, LanguageType = "no" });

            SetupSblMock();
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Get, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            NotificationSettingsResponse notificationAddresses = JsonSerializer.Deserialize<NotificationSettingsResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Equal(UserId, notificationAddresses.UserId);
            Assert.Equal(partyGuid, notificationAddresses.PartyUuid);
            Assert.Equal("test@example.com", notificationAddresses.EmailAddress);
            Assert.Equal("12345678", notificationAddresses.PhoneNumber);
            Assert.NotNull(notificationAddresses.ResourceIncludeList);
            Assert.Single(notificationAddresses.ResourceIncludeList);
            Assert.Equal("urn:altinn:resource:app_example", notificationAddresses.ResourceIncludeList[0]);
            Assert.True(notificationAddresses.NeedsConfirmation);
        }

        [Fact]
        public async Task GetNotificationAddress_WhenRepositoryReturnsNull_ReturnsNotFound()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _factory
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserPartyContactInfo)null);
            SetupSblMock();
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Get, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("Notification addresses not found", responseContent);
        }

        [Fact]
        public async Task GetNotificationAddress_WhenNoUserId_ReturnsUnauthorized()
        {
            // Arrange
            var partyGuid = Guid.NewGuid();

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllNotificationAddresses_WhenRepositoryReturnsEmpty_IsOkWithEmptyList()
        {
            const int UserId = 2516356;
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetAllNotificationAddressesForUserAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserPartyContactInfo>());
            SetupSblMock();

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Get, UserId, "profile/api/v1/users/current/notificationsettings/parties");
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var addresses = JsonSerializer.Deserialize<List<NotificationSettingsResponse>>(responseContent, _serializerOptionsCamelCase);
            Assert.Empty(addresses);
        }

        [Fact]
        public async Task GetAllNotificationAddresses_WhenRepositoryReturnsMultiple_IsOkWithList()
        {
            const int UserId = 2516357;
            var infos = new List<UserPartyContactInfo>
            {
                new UserPartyContactInfo { UserId = UserId, PartyUuid = Guid.NewGuid(), EmailAddress = "a@b.com", PhoneNumber = "1", UserPartyContactInfoResources = new List<UserPartyContactInfoResource> { new UserPartyContactInfoResource { ResourceId = "one" } } },
                new UserPartyContactInfo { UserId = UserId, PartyUuid = Guid.NewGuid(), EmailAddress = "c@d.com", PhoneNumber = "2", UserPartyContactInfoResources = new List<UserPartyContactInfoResource> { new UserPartyContactInfoResource { ResourceId = "two" } } }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetAllNotificationAddressesForUserAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(infos);
            SetupSblMock();
            _factory.ProfileSettingsRepositoryMock
                .Setup(x => x.GetProfileSettings(UserId))
                .ReturnsAsync(new ProfileSettings { UserId = UserId, IgnoreUnitProfileDateTime = DateTime.Today, LanguageType = "no" });

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Get, UserId, "profile/api/v1/users/current/notificationsettings/parties");
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var addresses = JsonSerializer.Deserialize<List<NotificationSettingsResponse>>(responseContent, _serializerOptionsCamelCase);
            Assert.Equal(2, addresses.Count);
            Assert.Equal("a@b.com", addresses[0].EmailAddress);
            Assert.Single(addresses[0].ResourceIncludeList);
            Assert.Equal("urn:altinn:resource:one", addresses[0].ResourceIncludeList[0]);
            Assert.Equal("c@d.com", addresses[1].EmailAddress);
            Assert.False(addresses[1].NeedsConfirmation);
        }

        [Fact]
        public async Task GetAllNotificationAddresses_WhenRepositoryThrowsException_ReturnsServerError()
        {
            const int UserId = 2516356;
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(x => x.GetAllNotificationAddressesForUserAsync(UserId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("fail"));
            SetupSblMock();

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Get, UserId, "profile/api/v1/users/current/notificationsettings/parties");
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task GetAllNotificationAddresses_WhenSystemUser_ReturnsBadRequest()
        {
            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequestWithSystemUser(HttpMethod.Get, "profile/api/v1/users/current/notificationsettings/parties");
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenUserIDoesNotHaveAccess_ReturnForbidden()
        {
            // Arrange
            var partyGuid = Guid.NewGuid();
            var userId = 2516356;

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "+4798765432",
            };

            HttpClient client = _factory.CreateClient();
            SetupAuthHandler(partyGuid, userId, false);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

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

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                EmailAddress = "test@@example.com",
                PhoneNumber = "++",
                ResourceIncludeList = ["example"]
            };
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                PhoneNumber = string.Empty,
                ResourceIncludeList = ["urn:altinn:resource:example"]
            };

            HttpClient client = _factory.CreateClient();
            SetupAuthHandler(partyGuid, UserId);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "+4798765432",
                ResourceIncludeList = [resource]
            };
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "+4798765432",
                ResourceIncludeList = ["urn:altinn:resource:example", "urn:altinn:resource:example"]
            };

            HttpClient client = _factory.CreateClient();
            SetupAuthHandler(partyGuid, UserId);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(content, _serializerOptionsCamelCase);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Single(actual.Errors);
            Assert.NotNull(actual.Errors["ResourceIncludeList"]);
            Assert.True(actual.Errors.TryGetValue("ResourceIncludeList", out var message));
            Assert.Contains("ResourceIncludeList cannot contain duplicates", message[0]);
        }

        [Theory]
        [InlineData("urn:altinn:resource:example", "example")]
        [InlineData("urn:altinn:resource:app_other_vale", "app_other_vale")]
        [InlineData("urn:altinn:resource:ttd-resource-1", "ttd-resource-1")]
        public async Task PutNotificationAddress_WhenContactInfoIsNew_ReturnsCreated(string resourceUrn, string sanitizedResourceId)
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "12345678",
                ResourceIncludeList = [resourceUrn]
            };

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(x => x.AddOrUpdateNotificationAddressAsync(It.IsAny<UserPartyContactInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            SetupSblMock();
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _factory.ProfessionalNotificationsRepositoryMock.Verify(
                x => x.AddOrUpdateNotificationAddressAsync(
                    It.Is<UserPartyContactInfo>(info =>
                        info.UserPartyContactInfoResources.Count == 1 && info.UserPartyContactInfoResources[0].ResourceId == sanitizedResourceId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PutNotificationAddress_WhenContactInfoAlreadyExists_ReturnsNoContent()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            var userPartyContactInfo = new NotificationSettingsRequest
            {
                EmailAddress = "test@example.com",
                PhoneNumber = "12345678",
                ResourceIncludeList = ["urn:altinn:resource:example"]
            };
            SetupAuthHandler(partyGuid, UserId);

            _factory
                .ProfessionalNotificationsRepositoryMock 
                .Setup(x => x.AddOrUpdateNotificationAddressAsync(It.IsAny<UserPartyContactInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            SetupSblMock();

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}")
            {
                Content = new StringContent(JsonSerializer.Serialize(userPartyContactInfo, _serializerOptionsCamelCase), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, UserId);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

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

            _factory
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.DeleteNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserPartyContactInfo)null);
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("Notification addresses not found", responseContent);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenRepositoryReturnsAddress_ReturnsOk()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _factory
                .ProfessionalNotificationsRepositoryMock
                .Setup(x => x.DeleteNotificationAddressAsync(UserId, partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPartyContactInfo());
            SetupAuthHandler(partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequestWithUserId(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/notificationsettings/parties/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Creates a request with a valid userId claim
        private static HttpRequestMessage CreateRequestWithUserId(HttpMethod method, int userId, string requestUri)
        {
            HttpRequestMessage httpRequestMessage = new(method, requestUri);
            httpRequestMessage = AddAuthHeadersToRequest(httpRequestMessage, userId);
            return httpRequestMessage;
        }

        // Creates a request with a system user token (no userId claim)
        private static HttpRequestMessage CreateRequestWithSystemUser(HttpMethod method, string requestUri)
        {
            string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
            HttpRequestMessage httpRequestMessage = new(method, requestUri);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }

        private static HttpRequestMessage AddAuthHeadersToRequest(HttpRequestMessage httpRequestMessage, int userId)
        {
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }

        private void SetupAuthHandler(Guid partyGuid, int UserId, bool access = true)
        {
            _factory.RegisterClientMock
                .Setup(x => x.GetPartyId(partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((int)partyGuid.GetHashCode()); // Simulate party ID retrieval
            _factory.AuthorizationClientMock
                .Setup(x => x.ValidateSelectedParty(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);
        }

        private void SetupSblMock()
        {
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction((request, token) =>
            {
                UserProfile userProfile = new() { ProfileSettingPreference = new ProfileSettingPreference { Language = "nb" } };
                return Task.FromResult(new HttpResponseMessage() { Content = JsonContent.Create(userProfile) });
            });
        }
    }
}
