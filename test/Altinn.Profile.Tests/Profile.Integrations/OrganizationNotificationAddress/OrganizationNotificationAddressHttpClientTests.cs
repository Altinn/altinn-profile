using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.OrganizationNotificationAddress;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests
{
    public class OrganizationNotificationAddressHttpClientTests
    {
        private readonly Mock<HttpMessageHandler> _messageHandler = new();

        [Fact]
        public async Task GetAddressChangesAsync_WhenMissingEndpointUrl_Throws()
        {
            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OrganizationNotificationAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(null));
        }

        [Fact]
        public async Task GetAddressChangesAsync_WhenEmptyEndpointUrl_Throws()
        {
            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OrganizationNotificationAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(string.Empty));
        }

        [Fact]
        public async Task GetAddressChangesAsync_WhenInvalidEndpointUrl_Throws()
        {
            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OrganizationNotificationAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync("notAnUrl"));
        }

        [Fact]
        public async Task GetAddressChangesAsync_WhenFailingToDeserialize_Throws()
        {
            NotificationAddressChangesLog changelog = new NotificationAddressChangesLog();
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create<NotificationAddressChangesLog>(changelog)
            };

            _messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OrganizationNotificationAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.GetAddressChangesAsync("http://example.com"));
        }

        [Fact]
        public async Task GetAddressChangesAsync_WhenNotSuccessResponse_Throws()
        {
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            };

            _messageHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(mockResponse);

            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OrganizationNotificationAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.GetAddressChangesAsync("http://example.com"));
        }

        [Fact]
        public async Task GetAddressChangesAsync_WhenValidEndpointUrl_Success()
        {
            NotificationAddressChangesLog changelog = new NotificationAddressChangesLog { OrganizationNotificationAddressList = [] };
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create<NotificationAddressChangesLog>(changelog)
            };

            _messageHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(mockResponse);

            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OrganizationNotificationAddressHttpClient(httpClient);

            var va = await client.GetAddressChangesAsync("http://example.com");

            _messageHandler.VerifyAll();
        }
    }
}
