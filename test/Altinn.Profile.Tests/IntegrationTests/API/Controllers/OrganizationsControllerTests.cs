using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class OrganizationsControllerTests : IClassFixture<WebApplicationFactory<OrganizationsController>>
    {
        private readonly WebApplicationFactorySetup<OrganizationsController> _webApplicationFactorySetup;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly List<Organization> _testdata;

        public OrganizationsControllerTests(WebApplicationFactory<OrganizationsController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<OrganizationsController>(factory);
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
                            Address = "test",
                            Domain = "test.com",
                            AddressType = AddressType.Email,
                        },
                        new()
                        {
                            FullAddress = "+4798765432",
                            Address = "98765432",
                            Domain = "+47",
                            AddressType = AddressType.SMS,
                        },
                        new()
                        {
                            FullAddress = "+4747765432",
                            Address = "47765432",
                            Domain = "+47",
                            AddressType = AddressType.SMS,
                        }
                    ]
                }
                ];
        }

        [Fact]
        public async Task GetMandatory_WhenOneOrganizationFound_ReturnsOkWithSingleItemList()
        {
            // Arrange
            var orgNo = "123456789";

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNo));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory");
            httpRequestMessage = CreateAutorizedRequest(orgNo, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<OrganizationResponse>(responseContent, _serializerOptions);
            Assert.Equal("123456789", actual.OrganizationNumber);
            Assert.Equal(3, actual.NotificationAddresses.Count);
            Assert.Equal("test@test.com", actual.NotificationAddresses[0].Email);
            Assert.Null(actual.NotificationAddresses[0].Phone);
            Assert.Equal("98765432", actual.NotificationAddresses[1].Phone);
            Assert.Equal("+47", actual.NotificationAddresses[1].CountryCode);
            Assert.Null(actual.NotificationAddresses[1].Email);
        }

        [Fact]
        public async Task GetMandatory_WhenNoMatchingOrganization_ReturnsNotFound()
        {
            // Arrange
            var orgNo = "error-org";

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory");
            httpRequestMessage = CreateAutorizedRequest(orgNo, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMandatory_WhenNoAuth_ReturnsUnautorized()
        {
            // Arrange
            var orgNo = "123456789";

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMandatory_WhenHavingWrongAccessToken_ReturnsForbidden()
        {
            // Arrange
            var orgNo = "123456789";

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory");
            string token = PrincipalUtil.GetOrgToken(orgNo);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static HttpRequestMessage CreateAutorizedRequest(string orgNo, HttpRequestMessage httpRequestMessage)
        {
            string token = PrincipalUtil.GetOrgToken(orgNo);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

            return httpRequestMessage;
        }
    }
}
