using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.User.PrivateConsent;

public class PrivateConsentClientTests
{
    private static PrivateConsentChangedRequest GetValidRequest() => new()
    {
        ChangeType = "insert",
        ChangeDateTime = DateTime.UtcNow,
        UserId = 123,
        PhoneNumber = "+4798765432",
        EmailAddress = "test@test.com"
    };

    private static PrivateConsentProfileClient CreateClient(HttpResponseMessage response, out Mock<ILogger<PrivateConsentProfileClient>> loggerMock)
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

        loggerMock = new Mock<ILogger<PrivateConsentProfileClient>>();

        return new PrivateConsentProfileClient(httpClient, loggerMock.Object, settingsMock.Object);
    }

    [Fact]
    public async Task UpdatePrivateConsent_SuccessfulRequest_DoesNotThrow()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var client = CreateClient(response, out _);

        var request = GetValidRequest();
        try
        {
            await client.UpdatePrivateConsent(request);
        }
        catch
        {
            Assert.Fail("Exception was thrown on successful request");
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task UpdatePrivateConsent_ServerError_ThrowsInternalServerErrorException(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);
        var client = CreateClient(response, out _);

        var request = GetValidRequest();
        await Assert.ThrowsAsync<InternalServerErrorException>(() => client.UpdatePrivateConsent(request));
    }

    [Fact]
    public async Task UpdatePrivateConsent_UnexpectedStatus_LogsError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request error")
        };
        var client = CreateClient(response, out var loggerMock);

        var request = GetValidRequest();
        await client.UpdatePrivateConsent(request);

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
