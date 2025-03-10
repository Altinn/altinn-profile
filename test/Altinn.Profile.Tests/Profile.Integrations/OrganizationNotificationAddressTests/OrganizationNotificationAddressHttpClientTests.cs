using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Platform.Profile.Models;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.Testdata;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class OrganizationNotificationAddressHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _messageHandler = new();

    [Fact]
    public async Task GetAddressChangesAsync_WhenMissingEndpointUrl_Throws()
    { 
        // Arrange
        var httpClient = new HttpClient();
        var client = new OrganizationNotificationAddressHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(null));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenEmptyEndpointUrl_Throws()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new OrganizationNotificationAddressHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(string.Empty));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenInvalidEndpointUrl_Throws()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new OrganizationNotificationAddressHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync("notAnUrl"));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenFailingToDeserialize_Throws()
    {
        // Arrange
        NotificationAddressChangesLog changelog = new NotificationAddressChangesLog();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create<NotificationAddressChangesLog>(changelog)
        };
        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult<HttpResponseMessage>(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = new OrganizationNotificationAddressHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.GetAddressChangesAsync("http://example.com"));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenNotSuccessResponse_Throws()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
        };
        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult<HttpResponseMessage>(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = new OrganizationNotificationAddressHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.GetAddressChangesAsync("http://example.com"));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenValidEndpointUrl_Success()
    {
        // Arrange
        NotificationAddressChangesLog changelog = new NotificationAddressChangesLog { OrganizationNotificationAddressList = [] };
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create<NotificationAddressChangesLog>(changelog)
        };

        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult<HttpResponseMessage>(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = new OrganizationNotificationAddressHttpClient(httpClient);

        // Act
        var va = await client.GetAddressChangesAsync("http://example.com");

        // Assert
        _messageHandler.VerifyAll();
    }
}
