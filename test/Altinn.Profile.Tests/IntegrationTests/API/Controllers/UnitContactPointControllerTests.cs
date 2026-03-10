using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UnitContactPointControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UnitContactPointControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;

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
    public async Task PostLookup_InvalidInputValues_ReturnsBadRequest(string input)
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
    public async Task PostLookup_ReturnsOkAndFilterOutBasedOnResourceID()
    {
        // Arrange
        UnitContactPointLookup input = new()
        {
            OrganizationNumbers = ["313441571"],
            ResourceId = "app_ttd_apps-test"
        };

        var client = _factory.CreateClient();

        var httpRequestMessage = CreateHttpRequestMessage(input);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
        Assert.Empty(actual.ContactPointsList);
    }

    [Fact]
    public async Task PostLookup_ReturnsOkAndIncludesBasedOnResourceID()
    {
        // Arrange
        UnitContactPointLookup input = new()
        {
            OrganizationNumbers = ["313441571"],
            ResourceId = "app_ttd_storage-end-to-end"
        };

        var client = _factory.CreateClient();

        var httpRequestMessage = CreateHttpRequestMessage(input);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
        Assert.Single(actual.ContactPointsList);
    }

    [Fact]
    public async Task PostLookup_ErrorResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        UnitContactPointLookup input = new()
        {
            OrganizationNumbers = ["error-org"],
            ResourceId = "app_ttd_apps-test"
        };

        var client = _factory.CreateClient();

        var httpRequestMessage = CreateHttpRequestMessage(input);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
        Assert.Empty(actual.ContactPointsList);
    }

    [Fact]
    public async Task PostLookup_WhenNoResponseFromRegister_ReturnsProblem()
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

        var httpRequestMessage = CreateHttpRequestMessage(input);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private HttpRequestMessage CreateHttpRequestMessage(UnitContactPointLookup input)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
        {
            Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
        };

        return httpRequestMessage;
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
                OrganizationNumbers = ["313441571"],
                ResourceId = originalResourceId
            };

            string actualResourceId = null;

            _factory.UnitContactPointsServiceMock.Setup(s => s.GetUserRegisteredContactPoints(
                    It.Is<string[]>(arr => arr.Length == 1 && arr[0] == "313441571"),
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

    public static List<Organization> GetRegisterResponse(string[] orgNo)
    {
        var parties = new List<Organization>();
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

    public static Organization GetPartyUuidForOrgNo(string orgNo)
    {
        return orgNo switch
        {
            "313605590" => Organization.Minimal(OrganizationIdentifier.Parse("313605590"), Guid.Parse("8baab949-07f9-4ac5-b8cb-af6208b59092")) with { PartyId = 50001 },
             "313441571" => Organization.Minimal(OrganizationIdentifier.Parse("313441571"), Guid.Parse("f81d7b22-4acb-4d59-b544-ef028f183ebc")) with { PartyId = 50002 },
            _ => null,
        };
    }
}
