using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Core.OrganizationNotificationAddresses;

using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class OrgContactPointControllerTests : IClassFixture<WebApplicationFactory<OrgContactPointController>>
    {
        private readonly WebApplicationFactorySetup<OrgContactPointController> _webApplicationFactorySetup;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly List<Organization> _testdata;

        public OrgContactPointControllerTests(WebApplicationFactory<OrgContactPointController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<OrgContactPointController>(factory);
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
                        new()
                        {
                            FullAddress = "+4798765433",
                            AddressType = AddressType.SMS,
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
                }
                ];
        }

        [Fact]
        public async Task PostLookup_WhenOneOrganizationFound_ReturnsOkWithSingleItemList()
        {
            // Arrange
            OrgContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789", "111111111"],
            };

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<OrgContactPointLookup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => input.OrganizationNumbers.Contains(o.OrganizationNumber)));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgContactPointsList>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_WhenMultipleOrganizationFound_ReturnsOkWithMultipleItemList()
        {
            // Arrange
            OrgContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789", "987654321"],
            };

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<OrgContactPointLookup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => input.OrganizationNumbers.Contains(o.OrganizationNumber)));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgContactPointsList>(responseContent, _serializerOptions);
            Assert.Equal(2, actual.ContactPointsList.Count);
        }

        [Fact]
        public async Task PostLookup_WhenNoMatchingOrganization_ReturnsEmptyList()
        {
            // Arrange
            OrgContactPointLookup input = new()
            {
                OrganizationNumbers = ["error-org"],
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/organizations/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrgContactPointsList>(responseContent, _serializerOptions);
            Assert.Empty(actual.ContactPointsList);
        }
    }
}
