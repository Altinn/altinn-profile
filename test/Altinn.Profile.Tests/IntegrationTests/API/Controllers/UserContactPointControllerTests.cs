using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.Testdata;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UserContactPointControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UserContactPointControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            string ssn = await request.Content.ReadAsStringAsync(token);
            return await GetSBlResponseForSsn(ssn);
        });

        SblBridgeSettings sblBrideSettings = new() { ApiProfileEndpoint = "http://localhost/" };
        _factory.SblBridgeSettingsOptions.Setup(s => s.Value).Returns(sblBrideSettings);
    }

    private async Task SeedTestData(string[] ssnList)
    {
        var users = new Dictionary<string, UserProfile>();
        foreach (string ssn in ssnList)
        {
            // Seed test data
            var user = await GetStoredDataForSsn(ssn);

            if (user == null)
            {
                continue;
            }

            users.Add(ssn, user);
        }

        _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([.. users.Select(u => new PersonContactPreferences
                {
                    NationalIdentityNumber = u.Key,
                    Email = u.Value.Email, 
                    IsReserved = u.Value.IsReserved,
                    LanguageCode = u.Value.ProfileSettingPreference.Language
                })]);
    }

    [Fact]
    public async Task PostAvailabilityLookup_NoNationalIdentityNumbers_EmptyListReturned()
    {
        // Arrange
        UserContactDetailsLookupCriteria input = new();

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/availability");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/availability");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/availability");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/lookup");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<UserContactPointsList>(responseContent, _serializerOptions);
        Assert.Empty(actual.ContactPointsList);
    }

    [Fact]
    public async Task PostLookup_SingleProfileNotFoundInBridge_RemainingUsersReturned()
    {
        // Arrange       
        UserContactDetailsLookupCriteria input = new()
        {
            NationalIdentityNumbers = new List<string>() { "01025101037", "01025101038", "99999999999" }
        };
        await SeedTestData(["01025101037", "01025101038"]);

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/lookup");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<UserContactPointsList>(responseContent, _serializerOptions);
        Assert.Equal(2, actual.ContactPointsList.Count);
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
        await SeedTestData(["01025101037"]);

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/users/contactpoint/lookup");

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<UserContactPointsList>(responseContent, _serializerOptions);
        Assert.Single(actual.ContactPointsList);
        Assert.NotEmpty(actual.ContactPointsList[0].Email);
    }

    private static async Task<UserProfile> GetStoredDataForSsn(string ssn)
    {
        UserProfile userProfile;

        switch (ssn)
        {
            case "01025101037":
                userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                return userProfile;
            case "01025101038":
                userProfile = await TestDataLoader.Load<UserProfile>("2001607");
                return userProfile;
            default:
                return null;
        }
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
