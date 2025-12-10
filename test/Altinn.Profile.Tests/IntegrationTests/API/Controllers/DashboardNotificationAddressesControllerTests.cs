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

        private readonly DateTime _testTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        private readonly List<Organization> _testdata;

        public DashboardNotificationAddressesControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
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
                            NotificationAddressID = 1,
                            RegistryUpdatedDateTime = _testTime,
                        },
                    ]
                },
                new()
                {
                    OrganizationNumber = "111111111",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@test.com",
                            AddressType = AddressType.Email,
                            NotificationAddressID = 1,
                            RegistryUpdatedDateTime = _testTime,
                        },
                        new()
                        {
                            FullAddress = "+4799999999",
                            AddressType = AddressType.SMS,
                            Address = "99999999",
                            Domain = "+47",
                            NotificationAddressID = 2,
                            RegistryUpdatedDateTime = _testTime,
                        },
                        new()
                        {
                            FullAddress = "+4798888888",
                            AddressType = AddressType.SMS,
                            Address = "98888888",
                            Domain = "+47",
                            NotificationAddressID = 3,
                            RegistryUpdatedDateTime = _testTime,
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
                            NotificationAddressID = 20,
                            RegistryUpdatedDateTime = _testTime,
                        },
                        new()
                        {
                            FullAddress = "+4791111111",
                            AddressType = AddressType.SMS,
                            HasRegistryAccepted = false,
                            NotificationAddressID = 21,
                            RegistryUpdatedDateTime = _testTime,
                        },
                        new()
                        {
                            FullAddress = "+4792222222",
                            AddressType = AddressType.SMS,
                            Address = "92222222",
                            Domain = "+47",
                            NotificationAddressID = 22,
                            RegistryUpdatedDateTime = _testTime,
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
            string orgNumber = "111111111";

            _factory.OrganizationNotificationAddressRepositoryMock
                 .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNumber));

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/notificationaddresses");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

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
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

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
            Assert.All(actual, a => Assert.Equal(orgNumber, a.SourceOrgNumber));
        }

        [Fact]
        public async Task GetAllNotificationAddresses_WhenReqOrgDoesntExist_ButParentOrgExists_ReturnsAddressesFromParent()
        {
            // Arrange
            string requestedOrg = "333333333";
            string parentOrg = "987654321";

            var parentUnit = _testdata.First(o => o.OrganizationNumber == parentOrg);

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.Is<List<string>>(a => a.Contains(requestedOrg)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Organization>());

            _factory.RegisterClientMock
                .Setup(r => r.GetMainUnit(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parentOrg);

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationAsync(parentOrg, It.IsAny<CancellationToken>()))
                .ReturnsAsync(parentUnit);

            HttpClient client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{requestedOrg}/notificationaddresses");
            request = CreateAuthorizedRequestWithScope(request);

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<List<DashboardNotificationAddressResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(actual);

            var expectedAddresses = parentUnit.NotificationAddresses?
                .Where(n => n.IsSoftDeleted != true && n.HasRegistryAccepted != false)
                .ToList() ?? new List<NotificationAddress>();

            Assert.Equal(expectedAddresses.Count, actual.Count);

            Assert.All(actual, a => Assert.Equal(requestedOrg, a.RequestedOrgNumber));
            Assert.All(actual, a => Assert.Equal(parentOrg, a.SourceOrgNumber));
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
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act            
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllNotificationAddressesForAnOrg_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string orgNumber = "111111111";

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/notificationaddresses");
            httpRequestMessage = GenerateTokenWithoutScope(orgNumber, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationAddressesByEmailAddress_WhenFound_ReturnsOkWithAddresses()
        {
            // Arrange
            string emailAddress = "test@test.com";

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(emailAddress, AddressType.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.NotificationAddresses != null &&
                    o.NotificationAddresses.Any(n => n.FullAddress == emailAddress && n.AddressType == AddressType.Email)));

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/email/{emailAddress}");

            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardNotificationAddressResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            var emailItem = result.FirstOrDefault(a => a.Email != null);
            Assert.NotNull(emailItem);
            Assert.Equal(emailAddress, emailItem.Email);
        }

        [Fact]
        public async Task GetNotificationAddressesByEmailAddress_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            string emailAddress = "missingtest@test.com";

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(It.IsAny<string>(), It.IsAny<AddressType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Organization>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/email/{emailAddress}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationAddressesByEmailAddress_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string emailAddress = "noaccess@test.com";

            _factory.OrganizationNotificationAddressRepositoryMock
          .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(It.IsAny<string>(), It.IsAny<AddressType>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(Enumerable.Empty<Organization>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/email/{emailAddress}");
            httpRequestMessage = GenerateTokenWithoutScope("any-org", httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationAddressesByPhoneNumber_WhenFound_ReturnsOkWithAddresses()
        {
            // Arrange
            string phoneNumber = "99999999";
            string countryCode = "+47";
            var encodedCountryCode = Uri.EscapeDataString(countryCode);
            string fullPhoneNumber = string.Concat(countryCode, phoneNumber);

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(fullPhoneNumber, AddressType.SMS, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.NotificationAddresses != null &&
                    o.NotificationAddresses.Any(n => n.FullAddress == fullPhoneNumber && n.AddressType == AddressType.SMS)));            

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/phonenumber/{phoneNumber}?countrycode={encodedCountryCode}");            

            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardNotificationAddressResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            var phoneItem = result.FirstOrDefault(a => a.Phone != null);
            Assert.NotNull(phoneItem);
            Assert.Equal(phoneNumber, phoneItem.Phone);
            Assert.Equal(countryCode, phoneItem.CountryCode);
        }

        [Fact]
        public async Task GetNotificationAddressesByPhoneNumber_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            string phoneNumber = "98888889";
            string countryCode = "+47";
            var encodedCountryCode = Uri.EscapeDataString(countryCode);

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(It.IsAny<string>(), It.IsAny<AddressType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Organization>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/phonenumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationAddressesByPhoneNumber_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string phoneNumber = "91919191";
            string countryCode = "+47";
            var encodedCountryCode = Uri.EscapeDataString(countryCode);

            _factory.OrganizationNotificationAddressRepositoryMock
          .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(It.IsAny<string>(), It.IsAny<AddressType>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(Enumerable.Empty<Organization>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/phonenumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = GenerateTokenWithoutScope("any-org", httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationAddressesByPhoneNumber_WhenCountryCodeProvided_ReturnsOk()
        {
            // Arrange
            string phoneNumber = "99999999";
            string countryCode = "+47";
            var encodedCountryCode = Uri.EscapeDataString(countryCode);
            string fullPhoneNumber = string.Concat(countryCode, phoneNumber);

            _factory.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationNotificationAddressesByFullAddressAsync(fullPhoneNumber, AddressType.SMS, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.NotificationAddresses != null &&
                    o.NotificationAddresses.Any(n => n.FullAddress == fullPhoneNumber && n.AddressType == AddressType.SMS)));

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/notificationaddresses/phonenumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardNotificationAddressResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            var phoneItem = result.FirstOrDefault(a => a.Phone != null);
            Assert.NotNull(phoneItem);
            Assert.Equal(phoneNumber, phoneItem.Phone);
            Assert.Equal(countryCode, phoneItem.CountryCode);
        }

        private static HttpRequestMessage GenerateTokenWithoutScope(string orgNumber, HttpRequestMessage httpRequestMessage)
        {
            string token = PrincipalUtil.GetOrgToken(orgNumber);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }

        private static HttpRequestMessage CreateAuthorizedRequestWithScope(HttpRequestMessage httpRequestMessage)
        {
            string token = PrincipalUtil.GetOrgToken("ttd", scope: "altinn:profile.support.admin");

            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }
    }
}
