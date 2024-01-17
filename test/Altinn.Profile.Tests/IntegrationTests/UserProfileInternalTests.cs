using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.Platform.Profile.Models;
using Altinn.Profile.Configuration;
using Altinn.Profile.Controllers;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Mocks;
using Altinn.Profile.Tests.Testdata;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests
{
    public class UserProfileInternalTests : IClassFixture<WebApplicationFactory<UserProfileInternalController>>
    {
        private readonly WebApplicationFactorySetup<UserProfileInternalController> _webApplicationFactorySetup;

        private readonly JsonSerializerOptions serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public UserProfileInternalTests(WebApplicationFactory<UserProfileInternalController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<UserProfileInternalController>(factory);

            GeneralSettings generalSettings = new() { BridgeApiEndpoint = "http://localhost/" };
            _webApplicationFactorySetup.GeneralSettingsOptions.Setup(s => s.Value).Returns(generalSettings);
        }

        [Fact]
        public async Task GetUserById_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
        {
            // Arrange
            const int UserId = 2516356;

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
                return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users/{UserId}", sblRequest.RequestUri.ToString());

            string responseContent = await response.Content.ReadAsStringAsync();

            UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
                responseContent, serializerOptionsCamelCase);

            // These asserts check that deserializing with camel casing was successful.
            Assert.Equal(UserId, actualUser.UserId);
            Assert.Equal("sophie", actualUser.UserName);
            Assert.Equal("Sophie Salt", actualUser.Party.Name);
            Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
            Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        }

        [Fact]
        public async Task GetUserByUuid_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
        {
            // Arrange
            Guid userUuid = new ("cc86d2c7-1695-44b0-8e82-e633243fdf31");

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
                return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users?useruuid={userUuid}", sblRequest.RequestUri.ToString());

            string responseContent = await response.Content.ReadAsStringAsync();

            UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
                responseContent, serializerOptionsCamelCase);

            // These asserts check that deserializing with camel casing was successful.
            Assert.Equal(userUuid, actualUser.UserUuid);
            Assert.Equal("LEO WILHELMSEN", actualUser.Party.Name);
            Assert.Equal("LEO", actualUser.Party.Person.FirstName);
            Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        }

        [Fact]
        public async Task GetUserById_SblBridgeReturnsNotFound_ResponseNotFound()
        {
            // Arrange
            const int UserId = 2222222;

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users/{UserId}", sblRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetUserByUuid_SblBridgeReturnsNotFound_ResponseNotFound()
        {
            // Arrange
            Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users?useruuid={userUuid}", sblRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetUserById_SblBridgeReturnsUnavailable_ResponseNotFound()
        {
            // Arrange
            const int UserId = 2222222;

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users/{UserId}", sblRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetUserByUuid_SblBridgeReturnsUnavailable_ResponseNotFound()
        {
            // Arrange
            Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users?useruuid={userUuid}", sblRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetUserBySsn_SblBridgeFindsProfile_ReturnsUserProfile()
        {
            // Arrange
            const string Ssn = "01017512345";
            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                UserProfile userProfile = await TestDataLoader.Load<UserProfile>("2516356");
                return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Post, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users", sblRequest.RequestUri.ToString());

            string requestContent = await sblRequest.Content.ReadAsStringAsync();

            Assert.Equal($"\"{Ssn}\"", requestContent);

            string responseContent = await response.Content.ReadAsStringAsync();

            UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
                responseContent, serializerOptionsCamelCase);

            // These asserts check that deserializing with camel casing was successful.
            Assert.Equal(2516356, actualUser.UserId);
            Assert.Equal("sophie", actualUser.UserName);
            Assert.Equal("Sophie Salt", actualUser.Party.Name);
            Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
            Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        }

        [Fact]
        public async Task GetUserBySsn_SblBridgeReturnsNotFound_RespondsNotFound()
        {
            // Arrange
            const string Ssn = "01017512345";
            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Post, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users", sblRequest.RequestUri.ToString());

            string requestContent = await sblRequest.Content.ReadAsStringAsync();

            Assert.Equal($"\"{Ssn}\"", requestContent);
        }

        [Fact]
        public async Task GetUserBySsn_SblBridgeReturnsUnavailable_RespondsNotFound()
        {
            // Arrange
            const string Ssn = "01017512345";
            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Post, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users", sblRequest.RequestUri.ToString());

            string requestContent = await sblRequest.Content.ReadAsStringAsync();

            Assert.Equal($"\"{Ssn}\"", requestContent);
        }

        [Fact]
        public async Task GetUserByUsername_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
        {
            // Arrange
            const string Username = "OrstaECUser";

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                UserProfile userProfile = await TestDataLoader.Load<UserProfile>(Username);
                return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users/?username={Username}", sblRequest.RequestUri.ToString());

            string responseContent = await response.Content.ReadAsStringAsync();

            UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
                responseContent, serializerOptionsCamelCase);

            // These asserts check that deserializing with camel casing was successful.
            Assert.Equal(Username, actualUser.UserName);
            Assert.Equal(50005545, actualUser.Party.PartyId);
            Assert.Equal("ORSTA OG HEGGEDAL ", actualUser.Party.Name);
            Assert.Equal("ORSTA OG HEGGEDAL", actualUser.Party.Organization.Name);
            Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        }

        [Fact]
        public async Task GetUserByUsername_SblBridgeReturnsNotFound_ResponseNotFound()
        {
            // Arrange
            const string Username = "NonExistingUsername";

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users/?username={Username}", sblRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetUserByUsername_SblBridgeReturnsUnavailable_ResponseNotFound()
        {
            // Arrange
            const string Username = "OrstaECUser";

            HttpRequestMessage sblRequest = null;
            DelegatingHandlerStub messageHandler = new(async (request, token) =>
            {
                sblRequest = request;

                return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
            });
            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            Assert.NotNull(sblRequest);
            Assert.Equal(HttpMethod.Get, sblRequest.Method);
            Assert.EndsWith($"sblbridge/profile/api/users/?username={Username}", sblRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task GetUserEmptyInputModel_UserProfileInternalController_ResponseBadRequest()
        {
            // Arrange
            UserProfileLookup emptyInputModel = new UserProfileLookup();

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", emptyInputModel);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUserNullInputModel_UserProfileInternalController_ResponseBadRequest()
        {
            // Arrange
            UserProfileLookup nullInputModel = null;

            HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", nullInputModel);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static HttpRequestMessage CreatePostRequest(string requestUri, UserProfileLookup lookupRequest)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri);
            httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(lookupRequest), Encoding.UTF8, "application/json");
            return httpRequestMessage;
        }
    }
}
