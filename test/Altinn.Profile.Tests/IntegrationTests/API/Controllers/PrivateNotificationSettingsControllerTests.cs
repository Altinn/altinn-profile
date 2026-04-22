using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class PrivateNotificationSettingsControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PrivateNotificationSettingsControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.UserContactInfoRepositoryMock.Reset();
        _factory.AddressVerificationRepositoryMock.Reset();
    }

    [Fact]
    public async Task PutPhoneNumber_WhenUnauthenticated_ReturnsUnauthorized()
    {
        HttpClient client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Put, "profile/api/v1/users/current/notificationsettings/private/phonenumber")
        {
            Content = CreateJsonContent(new PrivateNotificationSettingsUpdateRequest { Value = null })
        };

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutPhoneNumber_WhenSystemUser_ReturnsBadRequest()
    {
        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = new(HttpMethod.Put, "profile/api/v1/users/current/notificationsettings/private/phonenumber")
        {
            Content = CreateJsonContent(new PrivateNotificationSettingsUpdateRequest { Value = null })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetSystemUserToken(System.Guid.NewGuid()));

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutPhoneNumber_WhenAuthenticationMethodIsNotAllowed_ReturnsForbidden()
    {
        const int UserId = 2516356;

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = CreateRequestWithUserIdAndAuthMethod(HttpMethod.Put, UserId, "Mock", "profile/api/v1/users/current/notificationsettings/private/phonenumber", new PrivateNotificationSettingsUpdateRequest { Value = null });

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutPhoneNumber_WhenAddressIsUnverified_ReturnsUnprocessableEntity()
    {
        const int UserId = 2516356;

        _factory.AddressVerificationRepositoryMock
            .Setup(x => x.GetVerificationStatusAsync(UserId, AddressType.Sms, "+4798765432", It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerificationType.Unverified);

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = CreateRequestWithUserIdAndAuthMethod(HttpMethod.Put, UserId, "SelfIdentified", "profile/api/v1/users/current/notificationsettings/private/phonenumber", new PrivateNotificationSettingsUpdateRequest { Value = "+4798765432" });

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task PutPhoneNumber_WhenUserContactInfoIsNotFound_ReturnsNotFound()
    {
        const int UserId = 2516356;

        _factory.AddressVerificationRepositoryMock
            .Setup(x => x.GetVerificationStatusAsync(UserId, AddressType.Sms, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerificationType.Verified);

        _factory.UserContactInfoRepositoryMock
            .Setup(x => x.UpdatePhoneNumber(UserId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserContactInfo)null);

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = CreateRequestWithUserIdAndAuthMethod(HttpMethod.Put, UserId, "IdportenEpost", "profile/api/v1/users/current/notificationsettings/private/phonenumber", new PrivateNotificationSettingsUpdateRequest { Value = null });

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutPhoneNumber_WhenRequestIsValid_ReturnsOkAndUpdatedValue()
    {
        const int UserId = 2516356;
        const string phoneNumber = "+4798765432";

        _factory.AddressVerificationRepositoryMock
            .Setup(x => x.GetVerificationStatusAsync(UserId, AddressType.Sms, phoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VerificationType.Verified);

        _factory.UserContactInfoRepositoryMock
            .Setup(x => x.UpdatePhoneNumber(UserId, phoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserContactInfo
            {
                UserId = UserId,
                UserUuid = System.Guid.NewGuid(),
                Username = "test-user",
                CreatedAt = System.DateTime.UtcNow,
                EmailAddress = "test@example.com",
                PhoneNumber = phoneNumber
            });

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = CreateRequestWithUserIdAndAuthMethod(HttpMethod.Put, UserId, "SelfIdentified", "profile/api/v1/users/current/notificationsettings/private/phonenumber", new PrivateNotificationSettingsUpdateRequest { Value = phoneNumber });
        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<PrivateNotificationSettingsUpdateResponse>(content, _serializerOptionsCamelCase);

        Assert.NotNull(actual);
        Assert.Equal(phoneNumber, actual.Value);

        _factory.AddressVerificationRepositoryMock.Verify(x => x.GetVerificationStatusAsync(UserId, Core.AddressVerifications.Models.AddressType.Sms, phoneNumber, It.IsAny<CancellationToken>()), Times.Once);
        _factory.UserContactInfoRepositoryMock.Verify(x => x.UpdatePhoneNumber(UserId, phoneNumber, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PutPhoneNumber_WhenRequestIsValidAndContainsNull_ReturnsOkAndUpdatedValue()
    {
        const int UserId = 2516356;

        _factory.UserContactInfoRepositoryMock
            .Setup(x => x.UpdatePhoneNumber(UserId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserContactInfo
            {
                UserId = UserId,
                UserUuid = System.Guid.NewGuid(),
                Username = "test-user",
                CreatedAt = System.DateTime.UtcNow,
                EmailAddress = "test@example.com",
                PhoneNumber = null
            });

        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = CreateRequestWithUserIdAndAuthMethod(HttpMethod.Put, UserId, "SelfIdentified", "profile/api/v1/users/current/notificationsettings/private/phonenumber", new PrivateNotificationSettingsUpdateRequest { Value = null });

        HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var actual = JsonSerializer.Deserialize<PrivateNotificationSettingsUpdateResponse>(content, _serializerOptionsCamelCase);

        Assert.NotNull(actual);
        Assert.Null(actual.Value);

        _factory.UserContactInfoRepositoryMock.Verify(x => x.UpdatePhoneNumber(UserId, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static HttpRequestMessage CreateRequestWithUserIdAndAuthMethod(HttpMethod method, int userId, string authMethod, string requestUri, PrivateNotificationSettingsUpdateRequest body)
    {
        HttpRequestMessage request = new(method, requestUri)
        {
            Content = CreateJsonContent(body)
        };

        string token = PrincipalUtil.GetToken(userId, authMethod);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static StringContent CreateJsonContent<T>(T body)
    {
        return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    }
}
