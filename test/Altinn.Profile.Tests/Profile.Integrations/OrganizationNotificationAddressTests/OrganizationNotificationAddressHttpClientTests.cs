using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests;

public class OrganizationNotificationAddressHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _messageHandler = new();

    private static OrganizationNotificationAddressHttpClient CreateHttpClient(HttpClient httpClient)
    {
        return new OrganizationNotificationAddressHttpClient(httpClient, new OrganizationNotificationAddressSettings() { UpdateEndpoint = "https://example.com" });
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenMissingEndpointUrl_Throws()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = CreateHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(null));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenEmptyEndpointUrl_Throws()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = CreateHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(string.Empty));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenInvalidEndpointUrl_Throws()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = CreateHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync("notAnUrl"));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenFailingToDeserialize_Throws()
    {
        // Arrange
        NotificationAddressChangesLog changelog = new();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(changelog)
        };
        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

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
        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

        // Act and Assert
        await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.GetAddressChangesAsync("http://example.com"));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenValidEndpointUrl_Success()
    {
        // Arrange
        NotificationAddressChangesLog changelog = new() { OrganizationNotificationAddressList = [] };
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(changelog)
        };

        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

        // Act
        var va = await client.GetAddressChangesAsync("http://example.com");

        // Assert
        _messageHandler.VerifyAll();
    }

    [Fact]
    public async Task CreateNewAddress_WhenValid_Success()
    {
        // Arrange
        var response = new RegistryResponse();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(response)
        };

        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

        var notificationAddress = new NotificationAddress() { AddressType = AddressType.SMS, Address = "98765432", Domain = "+47" };

        // Act
        var va = await client.CreateNewNotificationAddress(notificationAddress, new Organization() { OrganizationNumber = "123456789" });

        // Assert
        Assert.IsType<RegistryResponse>(va);
        _messageHandler.VerifyAll();
    }

    [Fact]
    public async Task UpdateAddress_WhenSomethingGoesWrong_ThrowsException()
    {
        // Arrange
        var response = new RegistryResponse();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = JsonContent.Create(response)
        };

        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

        var notificationAddress = new NotificationAddress() { AddressType = AddressType.Email, Address = "test", Domain = "test.com" };

        // Act
        await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.UpdateNotificationAddress(notificationAddress, new Organization() { OrganizationNumber = "123456789" }));

        // Assert
        _messageHandler.VerifyAll();
    }

    [Fact]
    public async Task UpdateAddress_WhenValid_Success()
    {
        // Arrange
        var response = new RegistryResponse();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(response)
        };

        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

        var notificationAddress = new NotificationAddress() { AddressType = AddressType.Email, Address = "test", Domain = "test.com" };

        // Act
        var va = await client.UpdateNotificationAddress(notificationAddress, new Organization() { OrganizationNumber = "123456789" });

        // Assert
        Assert.IsType<RegistryResponse>(va);
        _messageHandler.VerifyAll();
    }

    [Fact]
    public async Task DeleteAddress_WhenValid_Success()
    {
        // Arrange
        var response = new RegistryResponse();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(response)
        };

        DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));

        var httpClient = new HttpClient(messageHandler);
        var client = CreateHttpClient(httpClient);

        // Act
        var va = await client.DeleteNotificationAddress(new NotificationAddress());

        // Assert
        Assert.IsType<RegistryResponse>(va);
        _messageHandler.VerifyAll();
    }
}
