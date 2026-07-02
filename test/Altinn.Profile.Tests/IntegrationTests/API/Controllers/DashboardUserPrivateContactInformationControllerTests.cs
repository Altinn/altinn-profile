using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public async Task GetContactInformationBySSN_WhenValidSSNProvided_ReturnsOkWithContactInformation()
        {
            // Arrange
            string ssn = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = ssn,
                Email = "user@example.com",
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastTouched = _testTime,
                EmailLastTouched = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Equal(ssn, result.NationalIdentityNumber);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("+4798765432", result.MobileNumber);
            Assert.False(result.IsReserved);
            Assert.Equal(_testTime, result.MobileNumberLastTouched);
            Assert.Equal(_testTime, result.EmailLastTouched);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenSSNNotFound_ReturnsNotFound()
        {
            // Arrange
            string ssn = "99999999999";

            // The mock returns empty list when SSN is not found
            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList<PersonContactPreferences>.Empty);

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenNoAccess_ReturnsForbidden()
        {
            // Arrange
            string ssn = "09861797993";

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithoutScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenSSNIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn = string.Empty };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenSSNIsNull_ReturnsBadRequest()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn = (string)null };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenEmailIsNull_ReturnsOkWithNullEmail()
        {
            // Arrange
            string ssn = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = ssn,
                Email = null,
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastTouched = _testTime,
                EmailLastTouched = null
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn = ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.Email);
            Assert.Equal("+4798765432", result.MobileNumber);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenMobileNumberIsNull_ReturnsOkWithNullMobileNumber()
        {
            // Arrange
            string ssn = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = ssn,
                Email = "user@example.com",
                MobileNumber = null,
                IsReserved = false,
                MobileNumberLastTouched = null,
                EmailLastTouched = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.MobileNumber);
            Assert.Equal("user@example.com", result.Email);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenUserIsReserved_ReturnsOkWithReservedFlag()
        {
            // Arrange
            string ssn = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = ssn,
                Email = "user@example.com",
                MobileNumber = "+4798765432",
                IsReserved = true,
                MobileNumberLastTouched = _testTime,
                EmailLastTouched = _testTime
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
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
        public async Task GetContactInformationBySSN_WhenBothEmailAndMobileAreNull_ReturnsOkWithContactInfo()
        {
            // Arrange
            string ssn = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = ssn,
                Email = null,
                MobileNumber = null,
                IsReserved = false,
                MobileNumberLastTouched = null,
                EmailLastTouched = null
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.Email);
            Assert.Null(result.MobileNumber);
            Assert.Equal(ssn, result.NationalIdentityNumber);
        }

        [Fact]
        public async Task GetContactInformationBySSN_WhenEmailLastTouchedIsNull_ReturnsOkWithNullEmailLastTouched()
        {
            // Arrange
            string ssn = "09861797993";
            var contactPreference = new PersonContactPreferences
            {
                NationalIdentityNumber = ssn,
                Email = "user@example.com",
                MobileNumber = "+4798765432",
                IsReserved = false,
                MobileNumberLastTouched = _testTime,
                EmailLastTouched = null
            };

            _factory.PersonServiceMock
                .Setup(s => s.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(contactPreference));

            HttpClient client = _factory.CreateClient();
            var requestBody = new { ssn };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/dashboard/users/contactinformation");
            httpRequestMessage.Content = JsonContent.Create(requestBody);
            httpRequestMessage = CreateAuthorizedRequestWithScope(httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<DashboardUserContactPointResponse>(responseContent, _serializerOptions);

            Assert.NotNull(result);
            Assert.Null(result.EmailLastTouched);
            Assert.Equal(_testTime, result.MobileNumberLastTouched);
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
