#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class CorrespondenceControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CorrespondenceControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        RegisterHttpMessageHandlerHelpers.SetupRegisterPartyQueryLookup(_factory, orgNumbers => GetRegisterResponse(orgNumbers));

        _factory.ProfessionalNotificationsRepositoryMock.Setup(s => s.GetAllNotificationAddressesForPartyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid partyUuid, CancellationToken _) => GetRepositoryResponse(partyUuid.ToString()));
        _factory.OrganizationNotificationAddressRepositoryMock.Setup(s => s.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<string> orgNumbers, CancellationToken _) => GetRepositoryResponse(orgNumbers));
    }

    [Theory]
    [InlineData("not deserialiable to input model")]
    [InlineData("{\"organizationNumbers\":null,\"resourceId\":\"resurs\"}")]
    [InlineData("{\"organizationNumbers\":[],\"resourceId\":\"resurs\"}")]
    [InlineData("{\"organizationNumbers\":[\"565445454\"],\"resourceId\":null}")]
    public async Task GetUserRegisteredContactPoints_InvalidInputValues_ReturnsBadRequest(string input)
    {
        // Arrange
        string token = PrincipalUtil.GetOrgToken("digdir", 4, "altinn:profile/correspondence.notificationsettings.read");

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/correspondence/units/contactpoint/lookup")
        {
            Content = new StringContent(input, System.Text.Encoding.UTF8, "application/json")
        };
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage actual = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
    }

    [Fact]
    public async Task GetUserRegisteredContactPoints_ReturnsOk()
    {
        // Arrange
        UnitContactPointLookup input = new()
        {
            OrganizationNumbers = ["123456789"],
            ResourceId = "app_ttd_apps-test"
        };

        string token = PrincipalUtil.GetOrgToken("digdir", 4, "altinn:profile/correspondence.notificationsettings.read");

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/correspondence/units/contactpoint/lookup")
        {
            Content = JsonContent.Create(input)
        };
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        UnitContactPointsList? actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
        Assert.Single(actual!.ContactPointsList);
    }

    [Theory]
    [InlineData("not deserialiable to input model")]
    [InlineData("{}")]
    [InlineData("{\"organizationNumbers\":null}")]
    [InlineData("{\"organizationNumbers\":[]}")]
    [InlineData("{\"organizationNumbers\":[\" \"]}")]
    public async Task GetOrganizationRegisteredContactPoints_InvalidInputValues_ReturnsBadRequest(string input)
    {
        // Arrange
        string token = PrincipalUtil.GetOrgToken("digdir", 4, "altinn:profile/correspondence.notificationsettings.read");

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/correspondence/organizations/notificationaddresses/lookup")
        {
            Content = new StringContent(input, System.Text.Encoding.UTF8, "application/json")
        };
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage actual = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationRegisteredContactPoints_ReturnsOk()
    {
        // Arrange
        OrgNotificationAddressRequest input = new()
        {
            OrganizationNumbers = ["123456789"]
        };

        string token = PrincipalUtil.GetOrgToken("digdir", 4, "altinn:profile/correspondence.notificationsettings.read");

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/correspondence/organizations/notificationaddresses/lookup")
        {
            Content = JsonContent.Create(input)
        };
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        OrgNotificationAddressesResponse? actual = 
            JsonSerializer.Deserialize<OrgNotificationAddressesResponse>(responseContent, _serializerOptions);
        Assert.Single(actual!.ContactPointsList);
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

    private static List<Organization> GetRepositoryResponse(List<string> organizationNumbers)
    {
        var organizations = new List<Organization>();

        foreach (var org in organizationNumbers)
        {
            organizations.Add(new Organization()
            {
                OrganizationNumber = org,
                NotificationAddresses = [
                    new NotificationAddress()
                    {
                        AddressType = AddressType.Email,
                        FullAddress = "navn@navnesen.no"
                    },
                    new NotificationAddress()
                    {
                        AddressType = AddressType.SMS,
                        FullAddress = "+4798765432"
                    }
                ]
            });
        }

        return organizations;
    }

    private static List<Party> GetRegisterResponse(string[] orgNo)
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

    private static Party? GetPartyUuidForOrgNo(string orgNo)
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
