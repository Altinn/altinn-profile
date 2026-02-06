using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UnitContactPointControllerTests
{
    public class UseSblBridge : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public UseSblBridge(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;

            _factory.InMemoryConfigurationCollection["GeneralSettings:LookupUnitContactPointsAtSblBridge"] = "true";

            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string requestString = await request.Content.ReadAsStringAsync(token);
                UnitContactPointLookup lookup = JsonSerializer.Deserialize<UnitContactPointLookup>(requestString, _serializerOptions);
                return GetSBlResponseFromSBL(lookup.OrganizationNumbers[0], _serializerOptions);
            });

            _factory.RegisterClientMock.Setup(s => s.GetPartyUuids(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string[] orgNumbers, CancellationToken _) => GetRegisterResponse(orgNumbers));
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_True_SuccessResult_ReturnsOk()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789"],
                ResourceId = "app_ttd_apps-test"
            };

            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_True_ErrorResult_ReturnsProblemDetails()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["error-org"],
                ResourceId = "app_ttd_apps-test"
            };

            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }

    public class DoNotUseSblBridge : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public DoNotUseSblBridge(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;

            _factory.InMemoryConfigurationCollection["GeneralSettings:LookupUnitContactPointsAtSblBridge"] = "false";

            _factory.RegisterClientMock.Reset();
            _factory.RegisterClientMock.Setup(s => s.GetPartyUuids(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string[] orgNumbers, CancellationToken _) => GetRegisterResponse(orgNumbers));

            _factory.ProfessionalNotificationsRepositoryMock.Setup(s => s.GetAllNotificationAddressesForPartyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid partyUuid, CancellationToken _) => GetRepositoryResponse(partyUuid.ToString()));
        }

        [Theory]
        [InlineData("not deserialiable to input model")]
        [InlineData("{\"organizationNumbers\":[null],\"resourceId\":null}")]
        [InlineData("{\"organizationNumbers\":null,\"resourceId\":\"resurs\"}")]
        [InlineData("{\"organizationNumbers\":[],\"resourceId\":\"resurs\"}")]
        public async Task PostLookup_SblBridgeFeatureFlag_InvalidInputValues_ReturnsBadRequest(string input)
        {
            // Arrange
            var client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(input, System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_False_ReturnsOk()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789"],
                ResourceId = "app_ttd_apps-test"
            };

            var client = _factory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_False_ReturnsOkAndFilterOutBasedOnResourceID()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["111111111"],
                ResourceId = "app_ttd_apps-test"
            };

            var client = _factory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Empty(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_False_ReturnsOkAndIncludesBasedOnResourceID()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["111111111"],
                ResourceId = "app_ttd_storage-end-to-end"
            };

            var client = _factory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_False_ErrorResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["error-org"],
                ResourceId = "app_ttd_apps-test"
            };

            var client = _factory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Empty(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_SblBridgeFeatureFlag_False_WhenNoResponseFromRegister_ReturnsProblem()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["error-org"],
                ResourceId = "app_ttd_apps-test"
            };

            _factory.RegisterClientMock.Setup(s => s.GetPartyUuids(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((string[] orgNumbers, CancellationToken _) => null);

            var client = _factory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private static List<UserPartyContactInfo> GetRepositoryResponse(string partyUuid)
        {
            switch (partyUuid)
            {
                case "8baab949-07f9-4ac5-b8cb-af6208b59092":
                    return
                        [
                            new UserPartyContactInfo()
                        {
                            UserId = 20001,
                            EmailAddress = "user@email.com",
                            PhoneNumber = "98765432",
                            PartyUuid = Guid.Parse("8baab949-07f9-4ac5-b8cb-af6208b59092"),
                        }
                        ];
                case "f81d7b22-4acb-4d59-b544-ef028f183ebc":
                    return
                        [
                            new UserPartyContactInfo()
                        {
                            UserId = 20002,
                            EmailAddress = "user2@email.com",
                            PhoneNumber = "98765432",
                            PartyUuid = Guid.Parse("f81d7b22-4acb-4d59-b544-ef028f183ebc"),
                            UserPartyContactInfoResources = [
                                new UserPartyContactInfoResource()
                                {
                                    ResourceId = "app_ttd_storage-end-to-end"
                                }
                            ]
                        }
                        ];
                default:
                    return [];
            }
        }
    }

    public class MockedUnitContactPointsService : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public MockedUnitContactPointsService(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.InMemoryConfigurationCollection["GeneralSettings:LookupUnitContactPointsAtSblBridge"] = "false";
            _factory.UnitContactPointsServiceMock ??= new Mock<IUnitContactPointsService>();
            _factory.UnitContactPointsServiceMock.Reset();
        }

        [Fact]
        public async Task PostLookup_RemovesUrnPrefixFromResourceId()
        {
            // Arrange
            var originalResourceId = "urn:altinn:resource:app_ttd_storage-end-to-end";
            var expectedSanitizedResourceId = "app_ttd_storage-end-to-end";
            var input = new UnitContactPointLookup
            {
                OrganizationNumbers = ["111111111"],
                ResourceId = originalResourceId
            };

            string actualResourceId = null;

            _factory.UnitContactPointsServiceMock.Setup(s => s.GetUserRegisteredContactPoints(
                    It.Is<string[]>(arr => arr.Length == 1 && arr[0] == "111111111"),
                    It.Is<string>(r => r == expectedSanitizedResourceId),
                    It.IsAny<CancellationToken>()))
                .Callback((string[] orgs, string resourceId, CancellationToken _) => actualResourceId = resourceId)
                .ReturnsAsync(new UnitContactPointsList { ContactPointsList = new List<UnitContactPoints>() });

            var client = _factory.CreateClient();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            var response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedSanitizedResourceId, actualResourceId);
        }
    }

    public static HttpResponseMessage GetSBlResponseFromSBL(string orgNo, JsonSerializerOptions serializerOptions)
    {
        switch (orgNo)
        {
            case "123456789":
                var input = new List<PartyNotificationContactPoints>()
                {
                    new PartyNotificationContactPoints()
                    {
                        ContactPoints = [new SblUserRegisteredContactPoint()
                        {
                            LegacyUserId = 20001,
                            Email = "user@email.com"
                        }
                         ],
                        LegacyPartyId = 50001,
                        OrganizationNumber = "123456789",
                        PartyId = Guid.NewGuid()
                    }
                };

                return new HttpResponseMessage() { Content = JsonContent.Create(input, options: serializerOptions), StatusCode = HttpStatusCode.OK };
            default:
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }

    public static List<Party> GetRegisterResponse(string[] orgNo)
    {
        var parties = new List<Party>();
        foreach (var org in orgNo)
        {
            var party = GetPartyUuidForOrgNo(org);
            if (party != null)
            {
                parties.Add(party);
            }
        }

        return parties;
    }

    public static Party GetPartyUuidForOrgNo(string orgNo)
    {
        return orgNo switch
        {
            "123456789" => new Party()
            {
                PartyId = 50001,
                OrganizationIdentifier = "123456789",
                PartyUuid = Guid.Parse("8baab949-07f9-4ac5-b8cb-af6208b59092"),
            },
            "111111111" => new Party()
            {
                PartyId = 50002,
                OrganizationIdentifier = "111111111",
                PartyUuid = Guid.Parse("f81d7b22-4acb-4d59-b544-ef028f183ebc"),
            },
            _ => null,
        };
    }
}
