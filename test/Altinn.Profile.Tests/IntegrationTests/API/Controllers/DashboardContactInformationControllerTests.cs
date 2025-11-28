using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;

using Moq;

using Xunit;

using RegisterParty = Altinn.Profile.Core.Unit.ContactPoints.Party;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class DashboardContactInformationControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        private readonly DateTime _testTime = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public DashboardContactInformationControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.MemoryCache.Clear();
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenMultipleUsersExist_ReturnsOkWithAllUsers()
        {
            // Arrange
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = "01010112345";
                    userProfile.Party.Name = "John Doe";
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }
                else if (uri.Contains("/users/1002"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001607");
                    userProfile.UserId = 1002;
                    userProfile.Party.SSN = "01010198765";
                    userProfile.Party.Name = "Jane Smith";
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string orgNumber = "123456789";
            Guid partyUuid = Guid.NewGuid();

            // Mock Register Client - translate org number to party UUID
            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            // Mock ProfessionalNotificationsRepository - return contact info for multiple users
            var contactInfos = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user1@example.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,
                    PartyUuid = partyUuid,
                    EmailAddress = "user2@example.com",
                    PhoneNumber = "+4798765433",
                    LastChanged = _testTime.AddDays(-1)
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfos);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var user1 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010112345");
            Assert.NotNull(user1);
            Assert.Equal("John Doe", user1.Name);
            Assert.Equal("user1@example.com", user1.Email);
            Assert.Equal("+4798765432", user1.Phone);
            Assert.Equal(_testTime, user1.LastChanged);

            var user2 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010198765");
            Assert.NotNull(user2);
            Assert.Equal("Jane Smith", user2.Name);
            Assert.Equal("user2@example.com", user2.Email);
            Assert.Equal("+4798765433", user2.Phone);
            Assert.Equal(_testTime.AddDays(-1), user2.LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOrgHasNoContactInfo_ReturnsEmptyList()
        {
            // Arrange
            string orgNumber = "987654321";
            Guid partyUuid = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOrgNotFound_ReturnsNotFound()
        {
            // Arrange
            string orgNumber = "999999999";

            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<RegisterParty>)null);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOrgNotFoundEmptyList_ReturnsNotFound()
        {
            // Arrange
            string orgNumber = "888888888";

            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string orgNumber = "123456789";

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithoutScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenUserProfileNotFound_SkipsUser()
        {
            // Arrange
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = "01010112345";
                    userProfile.Party.Name = "John Doe";
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }
                else if (uri.Contains("/users/1002"))
                {
                    // User 1002 returns 404 (not found)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string orgNumber = "111222333";
            Guid partyUuid = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            var contactInfos = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user1@example.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,  // This user will fail to load
                    PartyUuid = partyUuid,
                    EmailAddress = "user2@example.com",
                    PhoneNumber = "+4798765433",
                    LastChanged = _testTime
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfos);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            // Only user 1001 should be in the result (user 1002 was skipped)
            Assert.Single(result);
            Assert.Equal("01010112345", result[0].NationalIdentityNumber);
            Assert.Equal("John Doe", result[0].Name);
            Assert.Equal("user1@example.com", result[0].Email);
            Assert.Equal("+4798765432", result[0].Phone);
            Assert.Equal(_testTime, result[0].LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenMultiplePartiesFound_ReturnsInternalServerError()
        {
            // Arrange
            string orgNumber = "999888777";
            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid1, OrganizationIdentifier = orgNumber },
                new() { PartyId = 67890, PartyUuid = partyUuid2, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert - exception is caught by error handler and returns 500
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenUserPartyIsNull_SkipsUser()
        {
            // Arrange
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party = null; // Party is null - should skip this user
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string orgNumber = "555666777";
            Guid partyUuid = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            var contactInfos = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user@example.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfos);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            // User with null Party should be skipped, resulting in empty array
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenPartySsnAndNameAreNull_SkipsUser()
        {
            // Arrange
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = null; // SSN is null
                    userProfile.Party.Name = null; // Name is also null
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string orgNumber = "444555666";
            Guid partyUuid = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            var contactInfos = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user@example.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfos);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result); // User should be skipped due to incomplete Party data
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOnlySsnIsNull_ReturnsWithEmptySSN()
        {
            // Arrange
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = null; // SSN is null
                    userProfile.Party.Name = "Valid Name"; // Name is populated
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string orgNumber = "333444555";
            Guid partyUuid = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            var contactInfos = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user@example.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfos);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(string.IsNullOrEmpty(result[0].NationalIdentityNumber));
            Assert.Equal("Valid Name", result[0].Name);
            Assert.Equal("user@example.com", result[0].Email);
            Assert.Equal("+4798765432", result[0].Phone);
            Assert.Equal(_testTime, result[0].LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOnlyNameIsNull_SkipsUser()
        {
            // Arrange
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = "01010112345"; // SSN is populated
                    userProfile.Party.Name = null; // Name is null
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string orgNumber = "222333444";
            Guid partyUuid = Guid.NewGuid();

            var parties = new List<RegisterParty>
            {
                new() { PartyId = 12345, PartyUuid = partyUuid, OrganizationIdentifier = orgNumber }
            };
            _factory.RegisterClientMock
                .Setup(r => r.GetPartyUuids(It.Is<string[]>(arr => arr.Length == 1 && arr[0] == orgNumber), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parties);

            var contactInfos = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user@example.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfos);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result); // User should be skipped due to missing Name
        }

        [Fact]
        public async Task GetContactInformationByEmailAddress_WhenMultipleUsersExist_ReturnsOkWithAllUsers()
        {
            // Arrange - SBL Bridge returns user profile for identity enrichment
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = "01010112345";
                    userProfile.Party.Name = "John Doe";
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }
                else if (uri.Contains("/users/1002"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001607");
                    userProfile.UserId = 1002;
                    userProfile.Party.SSN = "01010198765";
                    userProfile.Party.Name = "Jane Smith";
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string email = "search@example.com";
            string orgNumber1 = "341341341";
            Guid partyUuid1 = Guid.NewGuid();

            string orgNumber2 = "352352352";
            Guid partyUuid2 = Guid.NewGuid();

            // Mock repository to return raw contact info (no identity)
            var contactInfosFromRepo = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid1,
                    EmailAddress = email,
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,
                    PartyUuid = partyUuid2,
                    EmailAddress = email,
                    PhoneNumber = "+4798765433",
                    LastChanged = _testTime.AddDays(-1)
                }
            };

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByEmailAddressAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfosFromRepo);

            // Mock register client used by service/controller to map partyUuid -> org number
            _factory.RegisterClientMock
                .Setup(r => r.GetOrganizationNumberByPartyUuid(partyUuid1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgNumber1);
            _factory.RegisterClientMock
                .Setup(r => r.GetOrganizationNumberByPartyUuid(partyUuid2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgNumber2);

            HttpClient client = _factory.CreateClient();
            string encodedEmail = Uri.EscapeDataString(email);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/email/{encodedEmail}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var user1 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010112345");
            Assert.NotNull(user1);
            Assert.Equal("John Doe", user1.Name);
            Assert.Equal(email, user1.Email);
            Assert.Equal("+4798765432", user1.Phone);
            Assert.Equal(orgNumber1, user1.OrganizationNumber);
            Assert.Equal(_testTime, user1.LastChanged);

            var user2 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010198765");
            Assert.NotNull(user2);
            Assert.Equal("Jane Smith", user2.Name);
            Assert.Equal(email, user2.Email);
            Assert.Equal("+4798765433", user2.Phone);
            Assert.Equal(orgNumber2, user2.OrganizationNumber);
            Assert.Equal(_testTime.AddDays(-1), user2.LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByEmailAddress_WhenEmailHasNoContactInfo_ReturnsNotFound()
        {
            // Arrange
            string email = "noone@example.com";

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByEmailAddressAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserPartyContactInfo>());

            HttpClient client = _factory.CreateClient();
            string encodedEmail = Uri.EscapeDataString(email);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/email/{encodedEmail}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);       
        }

        [Fact]
        public async Task GetContactInformationByEmailAddress_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string email = "search@example.com";

            HttpClient client = _factory.CreateClient();
            string encodedEmail = Uri.EscapeDataString(email);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/email/{encodedEmail}");
            httpRequestMessage = CreateAuthorizedRequestWithoutScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByEmailAddress_WhenUserProfileNotFound_SkipsUser()
        {
            // Arrange - SBL Bridge returns user profile for identity enrichment
            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("/users/1001"))
                {
                    var userProfile = await TestDataLoader.Load<UserProfile>("2001606");
                    userProfile.UserId = 1001;
                    userProfile.Party.SSN = "01010112345";
                    userProfile.Party.Name = "John Doe";
                    return new HttpResponseMessage { Content = JsonContent.Create(userProfile) };
                }
                else if (uri.Contains("/users/1002"))
                {
                    // User 1002 returns 404 (not found)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            string email = "search@example.com";
            string orgNumber = "123456789";
            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();

            var contactInfosFromRepo = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid1,
                    EmailAddress = email,
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,
                    PartyUuid = partyUuid2,
                    EmailAddress = email,
                    PhoneNumber = "+4798765433",
                    LastChanged = _testTime.AddDays(-1)
                }
            };

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByEmailAddressAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfosFromRepo);

            _factory.RegisterClientMock
                .Setup(r => r.GetOrganizationNumberByPartyUuid(partyUuid1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgNumber);
            _factory.RegisterClientMock
                .Setup(r => r.GetOrganizationNumberByPartyUuid(partyUuid2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgNumber);

            HttpClient client = _factory.CreateClient();
            string encodedEmail = Uri.EscapeDataString(email);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/email/{encodedEmail}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            // Only user 1001 should be present (1002 skipped)
            Assert.Single(result);
            Assert.Equal("01010112345", result[0].NationalIdentityNumber);
            Assert.Equal("John Doe", result[0].Name);
            Assert.Equal(email, result[0].Email);
            Assert.Equal("+4798765432", result[0].Phone);
            Assert.Equal(orgNumber, result[0].OrganizationNumber);
            Assert.Equal(_testTime, result[0].LastChanged);
        }

        private static HttpRequestMessage CreateAuthorizedRequestWithoutScope(HttpRequestMessage httpRequestMessage, string org = "ttd")
        {
            string token = PrincipalUtil.GetOrgToken(org);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }

        private static HttpRequestMessage CreateAuthorizedRequestWithScope(HttpRequestMessage httpRequestMessage, string org = "ttd")
        {
            string token = PrincipalUtil.GetOrgToken(org, scope: "altinn:profile.support.admin");

            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }
    }
}
