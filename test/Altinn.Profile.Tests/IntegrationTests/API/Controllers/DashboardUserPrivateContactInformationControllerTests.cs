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
                .ReturnsAsync(ImmutableList.Create(contactPreference));

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
                .ReturnsAsync(ImmutableList<PersonContactPreferences>.Empty);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateGetRequest(nin);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationByNIN_WhenNoAccess_ReturnsForbidden()
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
        public async Task GetContactInformationByNIN_WhenNINIsNull_ReturnsBadRequest()
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
                .ReturnsAsync(ImmutableList.Create(contactPreference));

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
                .ReturnsAsync(ImmutableList.Create(contactPreference));

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
                .ReturnsAsync(ImmutableList.Create(contactPreference));

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
                .ReturnsAsync(ImmutableList.Create(contactPreference));

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
                .ReturnsAsync(ImmutableList.Create(contactPreference));

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

        private static HttpRequestMessage CreateGetRequest(string nationalIdentityNumber)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Headers.Add("NationalIdentityNumber", nationalIdentityNumber);
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
