using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Controllers;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UserContactPointControllerTests : IClassFixture<WebApplicationFactory<UserContactPointController>>
{
    private readonly WebApplicationFactorySetup<UserContactPointController> _webApplicationFactorySetup;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UserContactPointControllerTests(WebApplicationFactory<UserContactPointController> factory)
    {
        _webApplicationFactorySetup = new WebApplicationFactorySetup<UserContactPointController>(factory);

        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = new DelegatingHandlerStub(async (request, token) =>
        {
            string ssn = await request.Content.ReadAsStringAsync(token);
            return await GetSBlResponseForSsn(ssn);
        });

        SblBridgeSettings sblBrideSettings = new() { ApiProfileEndpoint = "http://localhost/" };
        _webApplicationFactorySetup.SblBridgeSettingsOptions.Setup(s => s.Value).Returns(sblBrideSettings);
    }

    [Fact]
    public async Task PostAvailabilityLookup_NoNationalIdentityNumbers_EmptyListReturned()
    {
        // Arrange
        UserContactDetailsLookupCriteria input = new();

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/availability");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<UserContactPointAvailabilityList>(responseContent, _serializerOptions);
        Assert.Empty(actual.AvailabilityList);
    }

    [Fact]
    public async Task PostAvailabilityLookup_SingleUser_DetailsReturned()
    {
        // Arrange       
        UserContactDetailsLookupCriteria input = new()
        {
            NationalIdentityNumbers = new List<string>() { "01025101037" }
        };

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/availability");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<UserContactPointAvailabilityList>(responseContent, _serializerOptions);
        Assert.Single(actual.AvailabilityList);
        Assert.True(actual.AvailabilityList[0].EmailRegistered);
    }

    [Fact]
    public async Task PostAvailabilityLookup_SingleProfileNotFoundInBridge_RemainingUsersReturned()
    {
        // Arrange       
        UserContactDetailsLookupCriteria input = new()
        {
            NationalIdentityNumbers = new List<string>() { "01025101037", "99999999999" }
        };

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/availability");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<UserContactPointAvailabilityList>(responseContent, _serializerOptions);
        Assert.Single(actual.AvailabilityList);
        Assert.True(actual.AvailabilityList[0].EmailRegistered);
    }

    [Fact]
    public async Task PostLookup_NoNationalIdentityNumbers_EmptyListReturned()
    {
        // Arrange       
        UserContactDetailsLookupCriteria input = new()
        {
            NationalIdentityNumbers = new List<string>() { }
        };

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/lookup");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<UserContactPointsList>(responseContent, _serializerOptions);
        Assert.Empty(actual.ContactPointsList);
    }

    [Fact]
    public async Task PostLookup_SingleProfileNotFoundInBridge_RemainingUsersReturned()
    {
        // Arrange       
        UserContactDetailsLookupCriteria input = new()
        {
            NationalIdentityNumbers = new List<string>() { "01025101037", "99999999999" }
        };

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/lookup");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<UserContactPointsList>(responseContent, _serializerOptions);
        Assert.Single(actual.ContactPointsList);
        Assert.NotEmpty(actual.ContactPointsList[0].Email);
    }

    [Fact]
    public async Task PostLookup_SingleUser_DetailsReturned()
    {
        // Arrange       
        UserContactDetailsLookupCriteria input = new()
        {
            NationalIdentityNumbers = new List<string>() { "01025101037" }
        };

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/lookup");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<UserContactPointsList>(responseContent, _serializerOptions);
        Assert.Single(actual.ContactPointsList);
        Assert.NotEmpty(actual.ContactPointsList[0].Email);
    }

    private async Task<HttpResponseMessage> GetSBlResponseForSsn(string ssn)
    {
        UserProfile userProfile;

        switch (ssn)
        {
            case "\"01025101037\"":
                userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                return new HttpResponseMessage() { Content = JsonContent.Create(userProfile, options: _serializerOptions), StatusCode = HttpStatusCode.OK };
            case "\"01025101038\"":
                userProfile = await TestDataLoader.Load<UserProfile>("2001607");
                return new HttpResponseMessage() { Content = JsonContent.Create(userProfile, options: _serializerOptions), StatusCode = HttpStatusCode.OK };
            default:
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound };
        }
    }
}
