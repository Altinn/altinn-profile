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

    private static OrganizationNotificationAddressHttpClient CreateHttpClient(HttpResponseMessage mockResponse = null)
    {
        HttpClient httpClient;
        if (mockResponse != null)
        {
            DelegatingHandlerStub messageHandler = new((request, cancellationToken) => Task.FromResult(mockResponse));
            httpClient = new HttpClient(messageHandler);
        }
        else
        {
            httpClient = new HttpClient();
        }

        return new OrganizationNotificationAddressHttpClient(httpClient, new OrganizationNotificationAddressSettings() { UpdateEndpoint = "https://example.com" });
    }

    [Fact]
    public void GetInitialUrl_WhenNoLastChangedDate_ReturnsUrlWithoutSinceParameter()
    {
        // Arrange
        var client = CreateHttpClient();

        // Act and Assert
        var url = client.GetInitialUrl(null);

        // Assert
        Assert.Contains("pageSize=", url);
        Assert.DoesNotContain("since=", url);
    }

    [Fact]
    public void GetInitialUrl_WhenLastChangedDate_ReturnsUrlWithSinceParameter()
    {
        // Arrange
        var client = CreateHttpClient();

        // Act and Assert
        var url = client.GetInitialUrl(DateTime.Now);

        // Assert
        Assert.Contains("pageSize=", url);
        Assert.Contains("since=", url);
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenMissingEndpointUrl_Throws()
    {
        // Arrange
        var client = CreateHttpClient();

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(null));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenEmptyEndpointUrl_Throws()
    {
        // Arrange
        var client = CreateHttpClient();

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(string.Empty));
    }

    [Fact]
    public async Task GetAddressChangesAsync_WhenInvalidEndpointUrl_Throws()
    {
        // Arrange
        var client = CreateHttpClient();

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

        var client = CreateHttpClient(mockResponse);

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

        var client = CreateHttpClient(mockResponse);

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

        var client = CreateHttpClient(mockResponse);

        // Act
        _ = await client.GetAddressChangesAsync("http://example.com");

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

        var client = CreateHttpClient(mockResponse);

        var notificationAddress = new NotificationAddress() { AddressType = AddressType.SMS, Address = "98765432", Domain = "+47", RegistryID = Guid.NewGuid().ToString("N") };

        // Act
        var va = await client.CreateNewNotificationAddress(notificationAddress, "123456789");

        // Assert
        Assert.IsType<string>(va.RegistryId);
        Assert.Null(va.ErrorMessage);

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

        var client = CreateHttpClient(mockResponse);

        var notificationAddress = new NotificationAddress() { AddressType = AddressType.Email, Address = "test", Domain = "test.com", RegistryID = Guid.NewGuid().ToString("N") };

        // Act
        await Assert.ThrowsAsync<OrganizationNotificationAddressChangesException>(async () => await client.UpdateNotificationAddress(notificationAddress, "123456789"));

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

        var client = CreateHttpClient(mockResponse);

        var notificationAddress = new NotificationAddress() { AddressType = AddressType.Email, Address = "test", Domain = "test.com", RegistryID = Guid.NewGuid().ToString("N") };

        // Act
        var va = await client.UpdateNotificationAddress(notificationAddress, "123456789");

        // Assert
        // Assert
        Assert.IsType<string>(va.RegistryId);
        Assert.Null(va.ErrorMessage);
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

        var client = CreateHttpClient(mockResponse);

        // Act
        var va = await client.DeleteNotificationAddress(Guid.NewGuid().ToString("N"));

        // Assert
        // Assert
        Assert.IsType<string>(va.RegistryId);
        Assert.Null(va.ErrorMessage);
        _messageHandler.VerifyAll();
    }
}
