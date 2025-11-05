
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Interfaces;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class DashboardNotificationAddressesControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly DateTime _testTimeStamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        private readonly List<Organization> _testdata;

        public DashboardNotificationAddressesControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _testdata = [
                new()
                {
                    OrganizationNumber = "987654321",
                    AddressOrigin = "987654321",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@example.com",
                            AddressType = AddressType.Email,
                            NotificationAddressID = 1,
                           
                            RegistryUpdatedDateTime = _testTimeStamp,
                        },
                    ]
                }, 
                new()
                {
                    OrganizationNumber = "123456789",
                    AddressOrigin = "987654321",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@test.com",
                            AddressType = AddressType.Email,
                            NotificationAddressID = 1,
                            RegistryUpdatedDateTime = _testTimeStamp,
                        },
                        new()
                        {
                            FullAddress = "+4799999999",
                            AddressType = AddressType.SMS,
                            Address = "99999999",
                            Domain = "+47",
                            NotificationAddressID = 2,
                            RegistryUpdatedDateTime = _testTimeStamp,
                        },
                        new()
                        {
                            FullAddress = "+4798888888",
                            AddressType = AddressType.SMS,
                            Address = "98888888",
                            Domain = "+47",
                            NotificationAddressID = 3,
                            RegistryUpdatedDateTime = _testTimeStamp,
                        }
                    ]
                },
                new()
                {
                    OrganizationNumber = "222222222",
                    AddressOrigin = "987654321",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@test.com",
                            AddressType = AddressType.Email,
                            IsSoftDeleted = true,
                            NotificationAddressID = 20,
                            RegistryUpdatedDateTime = _testTimeStamp,
                        },
                        new()
                        {
                            FullAddress = "+4791111111",
                            AddressType = AddressType.SMS,
                            HasRegistryAccepted = false,
                            NotificationAddressID = 21,
                            RegistryUpdatedDateTime = _testTimeStamp,
                        },
                        new()
                        {
                            FullAddress = "+4792222222",
                            AddressType = AddressType.SMS,
                            Address = "92222222",
                            Domain = "+47",
                            NotificationAddressID = 22,
                            RegistryUpdatedDateTime = _testTimeStamp,
                        }
                    ]
                }
            ];

            _factory = factory;
            _factory.PdpMock ??= new Mock<IPDP>();
            _factory.PdpMock
              .Setup(pdp => pdp.GetDecisionForRequest(It.IsAny<XacmlJsonRequestRoot>()))
              .ReturnsAsync(new XacmlJsonResponse { Response = [new XacmlJsonResult { Decision = "Permit" }] });
            _factory.RegisterClientMock.Reset();
            _factory.OrganizationNotificationAddressRepositoryMock.Reset();
        }

        [Fact]
        public async Task GetAllNotificationAddressesForAnOrg_WhenOneOrganizationFound_ReturnsOkWithAllAddresses()
        {
            // Arrange
           string orgNumber = "123456789";           
     
           _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNumber));

           HttpClient client = _factory.CreateClient();

           HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/notificationaddresses");
           httpRequestMessage = CreateAuthorizedRequest(httpRequestMessage);

           // Act            
           HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

           // Assert
           Assert.Equal(HttpStatusCode.OK, response.StatusCode);
           string responseContent = await response.Content.ReadAsStringAsync();
           var result = JsonSerializer.Deserialize<List<DashboardNotificationAddressResponse>>(responseContent, _serializerOptions);

           Assert.NotNull(result);
           Assert.Equal(3, result.Count);

           var email = result.FirstOrDefault(a => a.Email != null);
           Assert.NotNull(email);
           Assert.Equal("test@test.com", email.Email);
        }

        [Fact]
        public async Task GetAllNotificationAddressesForAnOrg_WhenDeletedOrNotAccepted_FilteredOut()
        {
            // Arrange
            string orgNumber = "222222222";            

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNumber));

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/notificationaddresses");
            httpRequestMessage = CreateAuthorizedRequest(httpRequestMessage);

            // Act            
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<List<DashboardNotificationAddressResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(actual);
            
            Assert.Single(actual);
            Assert.Equal("92222222", actual[0].Phone);
            Assert.Equal("+47", actual[0].CountryCode);

            // verify organization numbers
            Assert.All(actual, a => Assert.Equal(orgNumber, a.RequestedOrgNumber));
            Assert.All(actual, a => Assert.Equal("987654321", a.SourceOrgNumber));
        }

        [Fact]
        public async Task GetAllNotificationAddressesForAnOrg_WhenNoMatchingOrganization_ReturnsNotFound()
        {
            // Arrange
            string orgNumber = "error-org";            

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Organization>());

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/notificationaddresses");
            httpRequestMessage = CreateAuthorizedRequest(httpRequestMessage);

            // Act            
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);          

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllNotificationAddressesForAnOrg_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string orgNumber = "123456789";
                        
            _factory.PdpMock.Setup(p => p.GetDecisionForRequest(It.IsAny<XacmlJsonRequestRoot>()))
                   .ReturnsAsync(new XacmlJsonResponse { Response = [new XacmlJsonResult { Decision = "NotApplicable" }] });
           
            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNumber));

            HttpClient client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/notificationaddresses");
            CreateAuthorizedRequest(orgNumber, request);

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static HttpRequestMessage CreateAuthorizedRequest(string orgNumber, HttpRequestMessage httpRequestMessage)
        {
            string token = PrincipalUtil.GetOrgToken(orgNumber);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }

        private static HttpRequestMessage CreateAuthorizedRequest(HttpRequestMessage httpRequestMessage)
        {            
            string token = PrincipalUtil.GetOrgToken("ttd", scope: "altinn:profile.support.admin");
            
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }
    }
}
