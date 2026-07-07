using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Models.Dashboard;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class DashboardUserPrivateContactInformationControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        private readonly DateTime _testTime = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public DashboardUserPrivateContactInformationControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.MemoryCache.Clear();
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenValidNINProvided_ReturnsOkWithContactInformation()
        {
            // Arrange
            string nin = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = nin,
                Email = "user@example.com",
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.Is<IEnumerable<string>>(n => n.Contains(nin)), It.IsAny<CancellationToken>()))
                .ReturnsAsync([contactPreference]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(nin, result.NationalIdentityNumber);
            Assert.Equal("user@example.com", result.EmailAddress);
            Assert.Equal("+4798765432", result.PhoneNumber);
            Assert.False(result.IsReserved);
            Assert.Equal(_testTime, result.PhoneNumberLastUpdatedOrVerified);
            Assert.Equal(_testTime, result.EmailLastUpdatedOrVerified);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenNINNotFound_ReturnsNotFound()
        {
            // Arrange
            string nin = "99999999999";

            // The mock returns empty list when NIN is not found
            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenRequestLacksScope_ReturnsForbidden()
        {
            // Arrange
            string nin = "09861797993";

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithoutScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenNINIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(string.Empty);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenRequestLacksNINHeader_ReturnsBadRequest()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("abcdefghijk")]
        [InlineData("1234567890")]
        [InlineData("123456789012")]
        public async Task GetContactInformationByNIN_WhenNINHasWrongFormat_ReturnsBadRequest(string nin)
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenEmailIsNull_ReturnsOkWithNullEmail()
        {
            // Arrange
            string nin = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = nin,
                Email = null,
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = null
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([contactPreference]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.EmailAddress);
            Assert.Equal("+4798765432", result.PhoneNumber);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenMobileNumberIsNull_ReturnsOkWithNullMobileNumber()
        {
            // Arrange
            string nin = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = nin,
                Email = "user@example.com",
                MobileNumber = null,
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = null,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([contactPreference]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.PhoneNumber);
            Assert.Equal("user@example.com", result.EmailAddress);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenUserIsReserved_ReturnsOkWithReservedFlag()
        {
            // Arrange
            string nin = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = nin,
                Email = "user@example.com",
                MobileNumber = "+4798765432",
                IsReserved = true,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([contactPreference]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.True(result.IsReserved);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenBothEmailAndMobileAreNull_ReturnsOkWithContactInfo()
        {
            // Arrange
            string nin = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = nin,
                Email = null,
                MobileNumber = null,
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = null,
                EmailLastUpdatedOrVerified = null
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([contactPreference]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.EmailAddress);
            Assert.Null(result.PhoneNumber);
            Assert.Equal(nin, result.NationalIdentityNumber);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenEmailLastUpdatedOrVerifiedIsNull_ReturnsOkWithNullEmailLastUpdatedOrVerified()
        {
            // Arrange
            string nin = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = nin,
                Email = "user@example.com",
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = null
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([contactPreference]);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.EmailLastUpdatedOrVerified);
            Assert.Equal(_testTime, result.PhoneNumberLastUpdatedOrVerified);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenValidEmailProvided_ReturnsOkWithContactInformation()
        {
            // Arrange
            string email = "user@example.com";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = "09861797993",
                Email = email,
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("09861797993", result[0].NationalIdentityNumber);
            Assert.Equal(email, result[0].EmailAddress);
            Assert.Equal("+4798765432", result[0].PhoneNumber);
            Assert.False(result[0].IsReserved);
            Assert.Equal(_testTime, result[0].PhoneNumberLastUpdatedOrVerified);
            Assert.Equal(_testTime, result[0].EmailLastUpdatedOrVerified);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenEmailNotFound_ReturnsOKWithEmptyList()
        {
            // Arrange
            string email = "notfound@example.com";

            // Both services return empty lists when email is not found
            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList<PersonContactPreferences>.Empty);

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenEmailRouteSegmentIsEmpty_ReturnsNotFound()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/profile/api/v1/dashboard/users/contactinformation/email/");
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _factory.PersonServiceMock.Verify(s => s.GetContactPreferencesByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _factory.UserContactInfoRepositoryMock.Verify(r => r.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string email = "user@example.com";

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithoutScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenMultipleUsersWithSameEmail_ReturnsListWithAllMatches()
        {
            // Arrange
            string email = "shared@example.com";
            var contactPreference1 = new PersonContactPreferences
            {
                NationalIdentityNumber = "09861797993",
                Email = email,
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            var contactPreference2 = new PersonContactPreferences
            {
                NationalIdentityNumber = "09861797994",
                Email = email,
                MobileNumber = "+4798765433",
                IsReserved = true,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference1, contactPreference2));

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("09861797993", result[0].NationalIdentityNumber);
            Assert.False(result[0].IsReserved);
            Assert.Equal("09861797994", result[1].NationalIdentityNumber);
            Assert.True(result[1].IsReserved);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenEmailHasNullMobileNumber_ReturnsOkWithNullMobileNumber()
        {
            // Arrange
            string email = "user@example.com";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = "09861797993",
                Email = email,
                MobileNumber = null,
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = null,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Null(result[0].PhoneNumber);
            Assert.Equal(email, result[0].EmailAddress);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenUserIsReserved_ReturnsOkWithReservedFlag()
        {
            // Arrange
            string email = "user@example.com";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = "09861797993",
                Email = email,
                MobileNumber = "+4798765432",
                IsReserved = true,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo>());

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].IsReserved);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenBothKRRAndSIUsersExist_ReturnsCombinedResults()
        {
            // Arrange
            string email = "combined@example.com";

            // KRR user (regular user)
            var krrContactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = "09861797993",
                Email = email,
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastUpdatedOrVerified = _testTime,
                EmailLastUpdatedOrVerified = _testTime
            };

            // SI user (self-identified user)
            var selfIdentifiedUser = new UserContactInfo
            {
                UserId = 1,
                UserUuid = Guid.NewGuid(),
                Username = "siuser",
                CreatedAt = _testTime,
                EmailAddress = email,
                PhoneNumber = "+4798765433",
                PhoneNumberLastChanged = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(krrContactPreference));

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo> { selfIdentifiedUser });

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // First result should be from KRR
            Assert.Equal("09861797993", result[0].NationalIdentityNumber);
            Assert.Equal("+4798765432", result[0].PhoneNumber);
            Assert.False(result[0].IsReserved);

            // Second result should be from SI user
            Assert.Equal("siuser", result[1].Username);
            Assert.Equal("+4798765433", result[1].PhoneNumber);
        }

        [Fact]
        public async Task GetContactInformationByEmail_WhenOnlySIUsersExist_ReturnsSIResults()
        {
            // Arrange
            string email = "sionly@example.com";

            // SI user (self-identified user)
            var selfIdentifiedUser = new UserContactInfo
            {
                UserId = 1,
                UserUuid = Guid.NewGuid(),
                Username = "siuser",
                CreatedAt = _testTime,
                EmailAddress = email,
                PhoneNumber = "+4798765433",
                PhoneNumberLastChanged = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList<PersonContactPreferences>.Empty);

            _factory.UserContactInfoRepositoryMock
                .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserContactInfo> { selfIdentifiedUser });

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetEmailRequest(email);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<List<DashboardUserContactPointResponse>>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Single(result);

            // Result should be from SI user
            Assert.Equal("siuser", result[0].Username);
            Assert.Equal("+4798765433", result[0].PhoneNumber);
        }

        private static HttpRequestMessage CreateGetRequest(string nationalIdentityNumber)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Headers.Add("NationalIdentityNumber", nationalIdentityNumber);
            return httpRequestMessage;
        }

        private static HttpRequestMessage CreateGetEmailRequest(string email)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/dashboard/users/contactinformation/email/{Uri.EscapeDataString(email)}");
            return httpRequestMessage;
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
