using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.ProfileSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.User.ProfileSettings;

public class ProfileSettingsClientTests
{
    private static ProfileSettingsChangedRequest GetValidRequest() => new()
    {
        ChangeType = "insert",
        ChangeDateTime = DateTime.UtcNow,
        UserId = 123,
        LanguageType = "en",
        DoNotPromptForParty = false,
        PreselectedPartyUuid = Guid.NewGuid(),
        ShowClientUnits = true,
        ShouldShowSubEntities = false,
        ShouldShowDeletedEntities = false,
        IgnoreUnitProfileDateTime = null
    };

    private static ProfileSettingsClient CreateClient(HttpResponseMessage response, out Mock<ILogger<ProfileSettingsClient>> loggerMock)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var settingsMock = new Mock<IOptions<SblBridgeSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { ApiProfileEndpoint = "http://localhost/" });

        loggerMock = new Mock<ILogger<ProfileSettingsClient>>();

        return new ProfileSettingsClient(httpClient, loggerMock.Object, settingsMock.Object);
    }

    [Fact]
    public async Task UpdatePortalSettings_SuccessfulRequest_DoesNotThrow()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var client = CreateClient(response, out _);

        var request = GetValidRequest();
        await client.UpdatePortalSettings(request);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task UpdatePortalSettings_ServerError_ThrowsInternalServerErrorException(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);
        var client = CreateClient(response, out _);

        var request = GetValidRequest();
        await Assert.ThrowsAsync<InternalServerErrorException>(() => client.UpdatePortalSettings(request));
    }

    [Fact]
    public async Task UpdatePortalSettings_UnexpectedStatus_LogsError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request error")
        };
        var client = CreateClient(response, out var loggerMock);

        var request = GetValidRequest();
        await client.UpdatePortalSettings(request);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected response")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
