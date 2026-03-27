using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ModelUtils;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Integrations.Register;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Register
{
    public class RegisterClientTests
    {
        private readonly Mock<IOptions<RegisterSettings>> _settingsMock;
        private readonly Mock<IAccessTokenGenerator> _tokenGenMock;
        private readonly Mock<ILogger<RegisterClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://api.test.local/";

        public RegisterClientTests()
        {
            _settingsMock = new Mock<IOptions<RegisterSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new RegisterSettings { ApiRegisterEndpoint = _testBaseUrl });

            _tokenGenMock = new Mock<IAccessTokenGenerator>();
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns("token");

            _loggerMock = new Mock<ILogger<RegisterClient>>();
        }

        [Fact]
        public void Constructor_BaseAddressIsSetFromSettings()
        {
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            Assert.Equal(new Uri(_testBaseUrl), _httpClient.BaseAddress);
        }

        [Fact]
        public async Task GetMainUnit_SingleEntry_ReturnsOrganizationRecord()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new LookupMainUnitResponse
            {
                Data = [new() { OrganizationIdentifier = "123456789" }]
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123456789", result);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.IsType<StringContent>(sentRequest.Content);
            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            JsonNode sentPayload = JsonNode.Parse(requestContent);
            string sentData = (string)sentPayload["data"];
            Assert.Equal("urn:altinn:organization:identifier-no:111111111", sentData);
            Assert.Equal(new Uri(_testBaseUrl + "v2/internal/parties/main-units"), sentRequest.RequestUri);
        }

        [Fact]
        public async Task GetMainUnit_WhenNothingFound_ReturnsNull()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new LookupMainUnitResponse
            {
                Data = []
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMainUnit_WhenRegisterReturns500_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);

            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMainUnit_WhenNotAbleToGenerateToken_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns((string)null);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
            handler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetPartyId_WhenValidAccessToken_SetsUpHttpClientCorrectly()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var expectedPartyId = 12345;
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>
            {
                new() { PartyId = expectedPartyId }
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyId(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpMethod.Get, sentRequest.Method);
            Assert.Contains("v1/parties/identifiers?uuids=" + partyUuid.ToString(), sentRequest.RequestUri.ToString());
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
        }

        [Fact]
        public async Task GetPartyId_WhenClientRespondsSuccessfully_ReturnsPartyId()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var expectedPartyId = 12345;
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>
            {
                new() { PartyId = expectedPartyId }
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyId(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPartyId, result);
        }

        [Fact]
        public async Task GetPartyId_WhenRegisterResponseHasMultipleElements_ReturnsFirst()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var expectedPartyId = 12345;
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>
            {
                new() { PartyId = expectedPartyId },
                new() { PartyId = 67890 } // Additional entry to simulate multiple results
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyId(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPartyId, result);
        }

        [Fact]
        public async Task GetPartyId_WhenNotAbleToGenerateToken_ReturnsNull()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns((string)null);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyId(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
            handler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetPartyId_WhenRegisterReturns500_ReturnsNull()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyId(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPartyId_WhenResponseIsEmpty_ReturnsNull()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>());
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyId(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPartyUuids_WhenValidOrgNumbers_ReturnsPartyList()
        {
            // Arrange
            var orgNumbers = new[] { "111111111", "222222222" };
            var expectedParties = new List<Altinn.Profile.Core.Unit.ContactPoints.Party>
            {
                new() { OrganizationIdentifier = "111111111", PartyUuid = Guid.NewGuid() },
                new() { OrganizationIdentifier = "222222222", PartyUuid = Guid.NewGuid() }
            };

            var responseContent = JsonSerializer.Serialize(new QueryPartiesResponse
            {
                Data = expectedParties
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyUuids(orgNumbers, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedParties.Count, result.Count);
            Assert.Equal(expectedParties[0].OrganizationIdentifier, result[0].OrganizationIdentifier);
            Assert.Equal(expectedParties[0].PartyUuid, result[0].PartyUuid);
            Assert.Equal(expectedParties[1].OrganizationIdentifier, result[1].OrganizationIdentifier);
            Assert.Equal(expectedParties[1].PartyUuid, result[1].PartyUuid);

            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v2/internal/parties/query?fields=id,uuid,org-id"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));

            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var sentPayload = JsonNode.Parse(requestContent);
            var sentData = sentPayload["data"].AsArray();
            Assert.Equal(2, sentData.Count);
            Assert.Equal("urn:altinn:organization:identifier-no:111111111", (string)sentData[0]);
            Assert.Equal("urn:altinn:organization:identifier-no:222222222", (string)sentData[1]);
        }

        [Fact]
        public async Task GetPartyUuids_WhenRegisterReturns500_ReturnsNull()
        {
            // Arrange
            var orgNumbers = new[] { "111111111" };
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyUuids(orgNumbers, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPartyUuids_WhenNotAbleToGenerateToken_ReturnsNull()
        {
            // Arrange
            var orgNumbers = new[] { "111111111" };
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns((string)null);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyUuids(orgNumbers, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
            handler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetPartyUuids_WhenResponseDataIsNull_ReturnsNull()
        {
            // Arrange
            var orgNumbers = new[] { "111111111" };
            var responseContent = JsonSerializer.Serialize(new QueryPartiesResponse
            {
                Data = null
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyUuids(orgNumbers, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPartyUuids_WhenResponseIsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var orgNumbers = new[] { "111111111" };
            var responseContent = JsonSerializer.Serialize(new QueryPartiesResponse
            {
                Data = new List<Altinn.Profile.Core.Unit.ContactPoints.Party>()
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetPartyUuids(orgNumbers, TestContext.Current.CancellationToken);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPartyUuids_WhenInvalidJsonResponse_ThrowsException()
        {
            // Arrange
            var orgNumbers = new[] { "111111111" };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(async () =>
                await client.GetPartyUuids(orgNumbers, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenValidAccessToken_SetsUpHttpClientCorrectly()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var expectedOrgNumber = "987654321";
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>
            {
                new() { OrgNumber = expectedOrgNumber }
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetOrganizationNumberByPartyUuid(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrgNumber, result);
            Assert.Equal(HttpMethod.Get, sentRequest.Method);
            Assert.Contains("v1/parties/identifiers?uuids=" + partyUuid.ToString(), sentRequest.RequestUri.ToString());
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenClientRespondsSuccessfully_ReturnsOrgNumber()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var expectedOrgNumber = "555666777";
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>
            {
                new() { OrgNumber = expectedOrgNumber }
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetOrganizationNumberByPartyUuid(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrgNumber, result);
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenRegisterResponseHasMultipleElements_ReturnsFirst()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var expectedOrgNumber = "341341341";
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>
            {
                new() { OrgNumber = expectedOrgNumber },
                new() { OrgNumber = "444555666" }
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetOrganizationNumberByPartyUuid(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrgNumber, result);
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenResponseIsEmpty_ReturnsNull()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var responseContent = JsonSerializer.Serialize(new List<PartyIdentifiersResponse>());
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetOrganizationNumberByPartyUuid(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenRegisterReturns500_ReturnsNull()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetOrganizationNumberByPartyUuid(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenNotAbleToGenerateToken_ReturnsNull()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string)null);

            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetOrganizationNumberByPartyUuid(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
            handler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetOrganizationNumberByPartyUuid_WhenCancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var client = new RegisterClient(new HttpClient(CreateHandler(new HttpResponseMessage(HttpStatusCode.OK)).Object), _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
            () => client.GetOrganizationNumberByPartyUuid(partyUuid, cts.Token));
        }

        [Fact]
        public async Task GetUserParty_ByUserId_WhenClientRespondsSuccessfully_ReturnsParty()
        {
            // Arrange
            var userId = 12345;
            var expectedParty = Person.Minimal("17902349936");
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            var result = await client.GetUserParty(userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedParty.PartyId, result.PartyId);
            Assert.Equal(expectedParty.Uuid, result.Uuid);
            Assert.Equal(expectedParty.Type, result.Type);
        }
                
        [Fact]
        public async Task GetUserParty_ByUserId_SetsUpRequestCorrectly()
        {
            // Arrange
            var userId = 12345;
            var expectedParty = Person.Minimal("17902349936");
            var (client, _, getCapturedRequest) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            await client.GetUserParty(userId, TestContext.Current.CancellationToken);
            var sentRequest = getCapturedRequest();

            // Assert
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Contains("v2/internal/parties/query?fields=person,party,user,si", sentRequest.RequestUri.ToString());
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));

            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var sentPayload = JsonNode.Parse(requestContent);
            var sentData = sentPayload["data"].AsArray();
            Assert.Single(sentData);
            Assert.Equal($"urn:altinn:user:id:{userId}", (string)sentData[0]);
        }

        [Fact]
        public async Task GetUserParty_ByUserId_WhenRegisterReturns500_ThrowsRegisterException()
        {
            // Arrange
            var userId = 12345;
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var (client, _, _) = CreateClientWithCapture(response);

            // Act & Assert
            await Assert.ThrowsAsync<RegisterException>(
                () => client.GetUserParty(userId, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task GetUserParty_ByUserId_WhenResponseDataIsNull_ReturnsNull()
        {
            // Arrange
            var userId = 12345;
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse());

            // Act
            var response = await client.GetUserParty(userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task GetUserParty_ByUserUuid_WhenClientRespondsSuccessfully_ReturnsParty()
        {
            // Arrange
            var userUuid = Guid.NewGuid();
            var expectedParty = Person.Minimal("17902349936", userUuid);
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            var result = await client.GetUserParty(userUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedParty.PartyId, result.PartyId);
            Assert.Equal(expectedParty.Uuid, result.Uuid);
            Assert.Equal(expectedParty.Type, result.Type);
        }

        [Fact]
        public async Task GetUserParty_ByUserUuid_SetsUpRequestCorrectly()
        {
            // Arrange
            var userUuid = Guid.NewGuid();
            var expectedParty = Person.Minimal("17902349936", userUuid);
            var (client, _, getCapturedRequest) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            await client.GetUserParty(userUuid, TestContext.Current.CancellationToken);
            var sentRequest = getCapturedRequest();

            // Assert
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Contains("v2/internal/parties/query?fields=person,party,user,si", sentRequest.RequestUri.ToString());
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));

            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var sentPayload = JsonNode.Parse(requestContent);
            var sentData = sentPayload["data"].AsArray();
            Assert.Single(sentData);
            Assert.Equal($"urn:altinn:party:uuid:{userUuid}", (string)sentData[0]);
        }

        [Fact]
        public async Task GetUserParty_ByUserUuid_WhenRegisterReturns500_ThrowsRegisterException()
        {
            // Arrange
            var userUuid = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var (client, _, _) = CreateClientWithCapture(response);

            // Act & Assert
            await Assert.ThrowsAsync<RegisterException>(
                () => client.GetUserParty(userUuid, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task GetUserParty_ByUserUuid_WhenResponseDataIsNull_ReturnsNull()
        {
            // Arrange
            var userUuid = Guid.NewGuid();
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse());

            // Act
            var response = await client.GetUserParty(userUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task GetUserParty_ByUsername_WhenClientRespondsSuccessfully_ReturnsParty()
        {
            // Arrange
            var username = "testuser";
            var expectedParty = SelfIdentifiedUser.MinimalLegacy(username);
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            var result = await client.GetUserPartyByUsername(username, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedParty.PartyId, result.PartyId);
            Assert.Equal(expectedParty.Uuid, result.Uuid);
            Assert.Equal(expectedParty.Type, result.Type);
        }

        [Fact]
        public async Task GetUserParty_ByUsername_SetsUpRequestCorrectly()
        {
            // Arrange
            var username = "testuser";
            var expectedParty = SelfIdentifiedUser.MinimalLegacy(username);
            var (client, _, getCapturedRequest) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            await client.GetUserPartyByUsername(username, TestContext.Current.CancellationToken);
            var sentRequest = getCapturedRequest();

            // Assert
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Contains("v2/internal/parties/query?fields=person,party,user,si", sentRequest.RequestUri.ToString());
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));

            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var sentPayload = JsonNode.Parse(requestContent);
            var sentData = sentPayload["data"].AsArray();
            Assert.Single(sentData);
            Assert.Equal($"urn:altinn:party:username:{username}", (string)sentData[0]);
        }

        [Fact]
        public async Task GetUserParty_BySsn_WhenClientRespondsSuccessfully_ReturnsParty()
        {
            // Arrange
            var ssn = "17902349936";
            var expectedParty = Person.Minimal(ssn);
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            var result = await client.GetUserPartyBySsn(ssn, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedParty.PartyId, result.PartyId);
            Assert.Equal(expectedParty.Uuid, result.Uuid);
            Assert.Equal(expectedParty.Type, result.Type);
        }

        [Fact]
        public async Task GetUserParty_BySsn_SetUpRequestCorrectly()
        {
            // Arrange
            var ssn = "17902349936";
            var expectedParty = Person.Minimal(ssn);
            var (client, _, getCapturedRequest) = CreateClientWithCapture(CreateQueryUserPartiesResponse(expectedParty));

            // Act
            await client.GetUserPartyBySsn(ssn, TestContext.Current.CancellationToken);
            var sentRequest = getCapturedRequest();

            // Assert
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Contains("v2/internal/parties/query?fields=person,party,user,si", sentRequest.RequestUri.ToString());
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));

            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var sentPayload = JsonNode.Parse(requestContent);
            var sentData = sentPayload["data"].AsArray();
            Assert.Single(sentData);
            Assert.Equal($"urn:altinn:person:identifier-no:{ssn}", (string)sentData[0]);
        }

        [Fact]
        public async Task GetUserParty_ByUserUuid_WhenResponseContainsOnlyOrganization_ReturnsNull()
        {
            // Arrange
            var userUuid = Guid.NewGuid();
            var (client, _, _) = CreateClientWithCapture(CreateQueryUserPartiesResponse(Organization.Minimal("314249879")));

            // Act
            var result = await client.GetUserParty(userUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        private static Mock<HttpMessageHandler> CreateHandler(
            HttpResponseMessage response,
            Action<HttpRequestMessage> requestCallback = null,
            Action<CancellationToken> cancelCallback = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    requestCallback?.Invoke(req);
                    cancelCallback?.Invoke(ct);
                    return response;
                });
            return handlerMock;
        }

        private static HttpResponseMessage CreateQueryUserPartiesResponse(params Altinn.Register.Contracts.Party[] parties)
        {
            var responseContent = JsonSerializer.Serialize(new QueryUserPartiesResponse
            {
                Data = parties.Length > 0 ? [.. parties] : null
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };
        }

        private (RegisterClient Client, Mock<HttpMessageHandler> Handler, Func<HttpRequestMessage> GetCapturedRequest) CreateClientWithCapture(HttpResponseMessage response)
        {
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            var httpClient = new HttpClient(handler.Object);
            var client = new RegisterClient(httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            return (client, handler, () => sentRequest);
        }
    }
}
