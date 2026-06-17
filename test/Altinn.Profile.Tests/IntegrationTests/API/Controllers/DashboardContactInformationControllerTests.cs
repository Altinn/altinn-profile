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

using Altinn.Authorization.ModelUtils;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;

using Moq;

using Xunit;

using static Altinn.Register.Contracts.PartyUrn;

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
            string orgNumber = "123456789";
            Guid partyUuid = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = CreateRegisterPerson(1002, "01010198765", "Jane Smith")
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid] = orgNumber
                });

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

            SetupRegisterPartyUuidsLookup(orgNumber, partyUuid);

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllNotificationAddressesForPartyAsync(partyUuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOrgNotFound_ReturnsEmptyList()
        {
            // Arrange
            string orgNumber = "888888888";

            SetupRegisterPartyUuidsLookup(orgNumber);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
           
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenUserProfileNotFound_SkipsUser()
        {
            // Arrange
            string orgNumber = "111222333";
            Guid partyUuid = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = null
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid] = orgNumber
                });

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

            SetupRegisterPartyUuidsLookup(orgNumber, partyUuid1, partyUuid2);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/{orgNumber}/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert - exception is caught by error handler and returns 500
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenUserPartyIsNull_SkipsUser()
        {
            // Arrange
            string orgNumber = "555666777";
            Guid partyUuid = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = null
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid] = orgNumber
                });

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            // User with null Party should be skipped, resulting in empty array
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenPartySsnAndNameAreNull_SkipsUser()
        {
            // Arrange
            string orgNumber = "444555666";
            Guid partyUuid = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterSelfIdentifiedUser(1001, string.Empty)
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid] = orgNumber
                });

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result); // User should be skipped due to incomplete Party data
        }

        [Fact]
        public async Task GetContactInformationByOrgNumber_WhenOnlySsnIsNull_ReturnsWithEmptySSN()
        {
            // Arrange
            string orgNumber = "333444555";
            Guid partyUuid = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterSelfIdentifiedUser(1001, "Valid Name")
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid] = orgNumber
                });

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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
            string orgNumber = "222333444";
            Guid partyUuid = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", string.Empty)
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid] = orgNumber
                });

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result); // User should be skipped due to missing Name
        }

        [Fact]
        public async Task GetContactInformationByEmailAddress_WhenMultipleUsersExist_ReturnsOkWithAllUsers()
        {
            // Arrange
            string email = "search@example.com";
            string orgNumber1 = "341341341";
            Guid partyUuid1 = Guid.NewGuid();

            string orgNumber2 = "352352352";
            Guid partyUuid2 = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = CreateRegisterPerson(1002, "01010198765", "Jane Smith")
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid1] = orgNumber1,
                    [partyUuid2] = orgNumber2
                });

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

            HttpClient client = _factory.CreateClient();
            string encodedEmail = Uri.EscapeDataString(email);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/email/{encodedEmail}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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
        public async Task GetContactInformationByEmailAddress_WhenEmailHasNoContactInfo_ReturnsEmptyList()
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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByEmailAddress_WhenUserProfileNotFound_SkipsUser()
        {
            // Arrange
            string email = "search@example.com";
            string orgNumber = "123456789";
            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = null
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid1] = orgNumber,
                    [partyUuid2] = orgNumber
                });

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

            HttpClient client = _factory.CreateClient();
            string encodedEmail = Uri.EscapeDataString(email);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/email/{encodedEmail}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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

        [Fact]
        public async Task GetContactInformationByPhoneNumber_WhenMultipleUsersExist_ReturnsOkWithAllUsers()
        {
            // Arrange
            string phoneNumber = "98765432";
            string countryCode = "+47";
            string encodedCountryCode = Uri.EscapeDataString(countryCode);
            string fullPhoneNumber = $"{countryCode}{phoneNumber}";

            string orgNumber1 = "341341341";
            Guid partyUuid1 = Guid.NewGuid();

            string orgNumber2 = "352352352";
            Guid partyUuid2 = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = CreateRegisterPerson(1002, "01010198765", "Jane Smith")
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid1] = orgNumber1,
                    [partyUuid2] = orgNumber2
                });

            // Mock repository to return raw contact info (no identity)
            var contactInfosFromRepo = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid1,
                    EmailAddress = "search@example1.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,
                    PartyUuid = partyUuid2,
                    EmailAddress = "search@example2.com",
                    PhoneNumber = "+4798765432",
                    LastChanged = _testTime.AddDays(-1)
                }
            };

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfosFromRepo);

            HttpClient client = _factory.CreateClient();
            string encodedPhoneNumber = Uri.EscapeDataString(phoneNumber);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var user1 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010112345");
            Assert.NotNull(user1);
            Assert.Equal("John Doe", user1.Name);
            Assert.Equal(fullPhoneNumber, user1.Phone);
            Assert.Equal(orgNumber1, user1.OrganizationNumber);
            Assert.Equal(_testTime, user1.LastChanged);

            var user2 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010198765");
            Assert.NotNull(user2);
            Assert.Equal("Jane Smith", user2.Name);
            Assert.Equal(fullPhoneNumber, user2.Phone);
            Assert.Equal(orgNumber2, user2.OrganizationNumber);
            Assert.Equal(_testTime.AddDays(-1), user2.LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByPhoneNumber_WhenContactHasNoContactInfo_ReturnsEmptyList()
        {
            // Arrange
            string phoneNumber = "98765432";
            string countryCode = "+47";
            string encodedCountryCode = Uri.EscapeDataString(countryCode);
            string fullPhoneNumber = $"{countryCode}{phoneNumber}";

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserPartyContactInfo>());

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContactInformationByPhoneNumber_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string phoneNumber = "98765432";
            string countryCode = "+47";
            string encodedCountryCode = Uri.EscapeDataString(countryCode);

            HttpClient client = _factory.CreateClient();
            
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithoutScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByPhoneNumber_WhenUserProfileNotFound_SkipsUser()
        {
            // Arrange
            string orgNumber = "123456789";

            string phoneNumber = "98765432";
            string countryCode = "+47";
            string encodedCountryCode = Uri.EscapeDataString(countryCode);
            string fullPhoneNumber = $"{countryCode}{phoneNumber}";

            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = null
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid1] = orgNumber,
                    [partyUuid2] = orgNumber
                });

            var contactInfosFromRepo = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid1,
                    EmailAddress = "search@example.com",
                    PhoneNumber = fullPhoneNumber,
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,
                    PartyUuid = partyUuid2,
                    EmailAddress = "search@example.com",
                    PhoneNumber = fullPhoneNumber,
                    LastChanged = _testTime.AddDays(-1)
                }
            };
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfosFromRepo);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);

            // Only user 1001 should be present (1002 skipped)
            Assert.Single(result);
            Assert.Equal("01010112345", result[0].NationalIdentityNumber);
            Assert.Equal("John Doe", result[0].Name);
            Assert.Equal("search@example.com", result[0].Email);
            Assert.Equal(fullPhoneNumber, result[0].Phone);
            Assert.Equal(orgNumber, result[0].OrganizationNumber);
            Assert.Equal(_testTime, result[0].LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByPhoneNumber_NoCountryCodeProvided_ReturnsOkWithNumbersWithoutCountryCode()
        {
            // Arrange
            // Phone provided without country code in request and repository stores phone without country code
            string phoneNumber = "92929292";
            string orgNumber1 = "341341341";
            Guid partyUuid1 = Guid.NewGuid();
            string orgNumber2 = "352352352";
            Guid partyUuid2 = Guid.NewGuid();

            SetupRegisterUserPartyAndOrganizationLookups(
                new Dictionary<int, Party>
                {
                    [1001] = CreateRegisterPerson(1001, "01010112345", "John Doe"),
                    [1002] = CreateRegisterPerson(1002, "01010198765", "Jane Smith")
                },
                new Dictionary<Guid, string>
                {
                    [partyUuid1] = orgNumber1,
                    [partyUuid2] = orgNumber2
                });

            var contactInfosFromRepo = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid1,
                    EmailAddress = "search1@example.com",
                    PhoneNumber = phoneNumber, // stored without country code
                    LastChanged = _testTime
                },
                new()
                {
                    UserId = 1002,
                    PartyUuid = partyUuid2,
                    EmailAddress = "search2@example.com",
                    PhoneNumber = phoneNumber,
                    LastChanged = _testTime.AddDays(-1)
                }
            };
            
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByPhoneNumberAsync(phoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfosFromRepo);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var user1 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010112345");
            Assert.NotNull(user1);
            Assert.Equal("John Doe", user1.Name);
            Assert.Equal(phoneNumber, user1.Phone);
            Assert.Equal(orgNumber1, user1.OrganizationNumber);
            Assert.Equal(_testTime, user1.LastChanged);

            var user2 = result.FirstOrDefault(u => u.NationalIdentityNumber == "01010198765");
            Assert.NotNull(user2);
            Assert.Equal("Jane Smith", user2.Name);
            Assert.Equal(phoneNumber, user2.Phone);
            Assert.Equal(orgNumber2, user2.OrganizationNumber);
            Assert.Equal(_testTime.AddDays(-1), user2.LastChanged);
        }

        [Fact]
        public async Task GetContactInformationByPhoneNumber_WithMismatchedCountryCodeFormat_ReturnsNoResults()
        {
            string phoneNumber = "92929292";
            string countryCode = "+47";
            string encodedCountryCode = Uri.EscapeDataString(countryCode);
            string fullPhoneNumber = $"{countryCode}{phoneNumber}";

            // Arrange - repository has phone number stored without country code
            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}?countrycode={encodedCountryCode}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("98765", "+47")] // 5 digits with country code
        [InlineData("98765432", "+47")] // 8 digits with country code
        [InlineData("98765432", "0047")] // 8 digits with country code
        [InlineData("98765", null)] // 5 digits without country code
        [InlineData("98765432", null)] // 8 digits without country code
        [InlineData("4798765432", null)] // Number with country code prefix (no +)
        [InlineData("004798765432", null)] // Number with 00 prefix
        [InlineData("12345", "+47")] // 5 digits with country code added
        [InlineData("987654321234546", "0047")] // 15 digits with country code
        [InlineData("8798765432", null)]        
        public async Task GetContactInformationByPhoneNumber_WithVariousPhoneNumberFormats_ReturnsOkWithUsers(string phoneNumber, string countryCode)
        {
            // Arrange
            var userId = 1001;
            Person registerPerson = Person.Minimal("14836498780") with
            {
                PartyId = 987654,
                Uuid = Guid.NewGuid(),
                ShortName = "John Doe",
                FirstName = "John",
                LastName = "Doe",
                ModifiedAt = DateTimeOffset.UtcNow,
                IsDeleted = false,
            };

            RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyByUserIdLookup(_factory, userId, registerPerson);

            string orgNumber = "341341341";
            Guid partyUuid = Guid.NewGuid();

            string searchPhoneNumber = string.IsNullOrEmpty(countryCode)
                ? phoneNumber
                : $"{countryCode}{phoneNumber}";

            var contactInfosFromRepo = new List<UserPartyContactInfo>
            {
                new()
                {
                    UserId = 1001,
                    PartyUuid = partyUuid,
                    EmailAddress = "user@example.com",
                    PhoneNumber = searchPhoneNumber,
                    LastChanged = _testTime
                }
            };

            _factory.ProfessionalNotificationsRepositoryMock
                .Setup(r => r.GetAllContactInfoByPhoneNumberAsync(searchPhoneNumber, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactInfosFromRepo);

            SetupRegisterUserPartyAndOrganizationLookups(
            new Dictionary<int, Party>
            {
                [1001] = registerPerson
            }, 
            new Dictionary<Guid, string>
            {
                [partyUuid] = orgNumber
            });

            HttpClient client = _factory.CreateClient();

            string requestUrl = string.IsNullOrEmpty(countryCode)
                ? $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}"
                : $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{phoneNumber}?countrycode={Uri.EscapeDataString(countryCode)}";

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, requestUrl);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactInformationResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("14836498780", result[0].NationalIdentityNumber);
            Assert.Equal("John Doe", result[0].Name);
            Assert.Equal("user@example.com", result[0].Email);
            Assert.Equal(searchPhoneNumber, result[0].Phone);
            Assert.Equal(orgNumber, result[0].OrganizationNumber);
            Assert.Equal(_testTime, result[0].LastChanged);
        }

        [Theory]
        [InlineData("1234", "+47")] // Too few digits (less than 5)
        [InlineData("12345678901234567", "+47")] // Too many digits (more than 15)
        [InlineData("abc12345", "0047")] // Contains letters
        [InlineData("123-456-7890", null)] // Contains hyphens
        [InlineData("123 456 7890", null)] // Contains spaces
        [InlineData("9876543210234546", null)] // 15 digits with country code
        public async Task GetContactInformationByPhoneNumber_WithInvalidPhoneNumberFormat_ReturnsBadRequest(string searchPhoneNumber, string countryCode)
        {
            // Arrange
            HttpClient client = _factory.CreateClient();

            string requestUrl = string.IsNullOrEmpty(countryCode)
                ? $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{searchPhoneNumber}"
                : $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{searchPhoneNumber}?countrycode={Uri.EscapeDataString(countryCode)}";

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/organizations/contactinformation/phoneNumber/{searchPhoneNumber}");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private void SetupRegisterPartyUuidsLookup(string orgNumber, params Guid[] partyUuids)
        {
            var lookup = partyUuids.ToDictionary(partyUuid => partyUuid, _ => orgNumber);
            _factory.RegisterHttpMessageHandler.ChangeHandlerFunction(
                RegisterHttpMessageHandlerHelpers.CreateCombinedRegisterPartyQueryAndIdentifiersHandler(lookup));
        }

        private void SetupRegisterUserPartyLookups(Dictionary<int, Party> userPartiesByUserId)
        {
            SetupRegisterUserPartyAndOrganizationLookups(userPartiesByUserId, new Dictionary<Guid, string>());
        }

        private void SetupRegisterUserPartyAndOrganizationLookups(Dictionary<int, Party> userPartiesByUserId, Dictionary<Guid, string> organizationNumbersByPartyUuid)
        {
            RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyByUserIdWithPartyQueryAndIdentifiersLookup(
                _factory,
                userPartiesByUserId,
                organizationNumbersByPartyUuid);
        }

        private static Person CreateRegisterPerson(int userId, string ssn, string name)
        {
            string[] splitName = name.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string firstName = splitName.Length > 0 ? splitName[0] : string.Empty;
            string lastName = splitName.Length > 1 ? splitName[1] : string.Empty;

            return Person.Minimal(ssn) with
            {
                PartyId = (uint)userId,
                Uuid = Guid.NewGuid(),
                ShortName = name,
                FirstName = firstName,
                LastName = lastName,
                ModifiedAt = DateTimeOffset.UtcNow,
                IsDeleted = false,
            };
        }

        private static SelfIdentifiedUser CreateRegisterSelfIdentifiedUser(int userId, string displayName)
        {
            return SelfIdentifiedUser.MinimalLegacy(displayName) with
            {
                PartyId = (uint)userId,
                Uuid = Guid.NewGuid(),
                User = new PartyUser((uint)userId, null, ImmutableValueArray<uint>.Empty.Add((uint)userId)),
                ModifiedAt = DateTimeOffset.UtcNow,
                IsDeleted = false,
            };
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
