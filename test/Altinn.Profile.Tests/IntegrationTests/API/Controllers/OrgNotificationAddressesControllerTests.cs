using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class OrgNotificationAddressesControllerTests : IClassFixture<WebApplicationFactory<OrgNotificationAddressController>>
    {
        private readonly WebApplicationFactorySetup<OrgNotificationAddressController> _webApplicationFactorySetup;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly List<Organization> _testdata;

        public OrgNotificationAddressesControllerTests(WebApplicationFactory<OrgNotificationAddressController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<OrgNotificationAddressController>(factory);
            _testdata = [
                new()
                {
                    OrganizationNumber = "987654321",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@example.com",
                            AddressType = AddressType.Email,
                        },
                    ]
                }, new()
                {
                    OrganizationNumber = "123456789",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@test.com",
                            AddressType = AddressType.Email,
                        },
                        new()
                        {
                            FullAddress = "+4798765432",
                            AddressType = AddressType.SMS,
                        },
                        new()
                        {
                            FullAddress = "+4747765432",
                            AddressType = AddressType.SMS,
                        }
                    ]
                },
                new()
                {
                    OrganizationNumber = "222222222",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@test.com",
                            AddressType = AddressType.Email,
                            IsSoftDeleted = true,
                        },
                        new()
                        {
                            FullAddress = "+4798765432",
                            AddressType = AddressType.SMS,
                            HasRegistryAccepted = false,
                        },
                        new()
                        {
                            FullAddress = "+4747765432",
                            AddressType = AddressType.SMS,
                        }
                    ]
                }

                ];
        }

        [Fact]
        public async Task PostLookup_WhenOneOrganizationFound_ReturnsOkWithSingleItemList()
        {
            // Arrange
            OrgNotificationAddressRequest input = new()
            {
                OrganizationNumbers = ["123456789", "111111111"],
            };

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => input.OrganizationNumbers.Contains(o.OrganizationNumber)));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/notificationaddresses/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgNotificationAddressesResponse>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
            Assert.Equal("123456789", actual.ContactPointsList[0].OrganizationNumber);
            Assert.Equal("123456789", actual.ContactPointsList[0].AddressOrigin);
            Assert.Single(actual.ContactPointsList[0].EmailList);
            Assert.Equal(2, actual.ContactPointsList[0].MobileNumberList.Count);
        }

        [Fact]
        public async Task PostLookup_WhenParentOrganizationFound_ReturnsOkWithSingleItemList()
        {
            // Arrange
            OrgNotificationAddressRequest input = new()
            {
                OrganizationNumbers = ["333333333", "111111111"],
            };

            _webApplicationFactorySetup.RegisterClientMock
                .Setup(r => r.GetMainUnit(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("123456789");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .SetupSequence(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => input.OrganizationNumbers.Contains(o.OrganizationNumber)))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == "123456789"))
                .ReturnsAsync([]);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/notificationaddresses/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgNotificationAddressesResponse>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
            Assert.Equal("333333333", actual.ContactPointsList[0].OrganizationNumber);
            Assert.Equal("123456789", actual.ContactPointsList[0].AddressOrigin);
            Assert.Single(actual.ContactPointsList[0].EmailList);
            Assert.Equal(2, actual.ContactPointsList[0].MobileNumberList.Count);
        }

        [Fact]
        public async Task PostLookup_WhenMultipleOrganizationFound_ReturnsOkWithMultipleItemList()
        {
            // Arrange
            OrgNotificationAddressRequest input = new()
            {
                OrganizationNumbers = ["123456789", "987654321"],
            };

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => input.OrganizationNumbers.Contains(o.OrganizationNumber)));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/notificationaddresses/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgNotificationAddressesResponse>(responseContent, _serializerOptions);
            Assert.Equal(2, actual.ContactPointsList.Count);
            Assert.Equal("987654321", actual.ContactPointsList[0].OrganizationNumber);
            Assert.Equal("987654321", actual.ContactPointsList[0].AddressOrigin);
            Assert.Single(actual.ContactPointsList[0].EmailList);
            Assert.Empty(actual.ContactPointsList[0].MobileNumberList);
            Assert.Equal("123456789", actual.ContactPointsList[1].OrganizationNumber);
            Assert.Equal("123456789", actual.ContactPointsList[1].AddressOrigin);
            Assert.Single(actual.ContactPointsList[1].EmailList);
            Assert.Equal(2, actual.ContactPointsList[1].MobileNumberList.Count);
        }

        [Fact]
        public async Task PostLookup_WhenOneOrganizationFoundWithDeletedAddress_ReturnsOkWithSingleItemListWithFilteredAddresses()
        {
            // Arrange
            OrgNotificationAddressRequest input = new()
            {
                OrganizationNumbers = ["222222222"],
            };

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => input.OrganizationNumbers.Contains(o.OrganizationNumber)));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/notificationaddresses/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgNotificationAddressesResponse>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
            Assert.Equal("222222222", actual.ContactPointsList[0].OrganizationNumber);
            Assert.Equal("222222222", actual.ContactPointsList[0].AddressOrigin);
            Assert.Empty(actual.ContactPointsList[0].EmailList);
            Assert.Single(actual.ContactPointsList[0].MobileNumberList);
        }

        [Fact]
        public async Task PostLookup_WhenNoMatchingOrganization_ReturnsEmptyList()
        {
            // Arrange
            OrgNotificationAddressRequest input = new()
            {
                OrganizationNumbers = ["error-org"],
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/notificationaddresses/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgNotificationAddressesResponse>(responseContent, _serializerOptions);
            Assert.Empty(actual.ContactPointsList);
        }
    }
}
