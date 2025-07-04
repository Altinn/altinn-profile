﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Interfaces;
using Altinn.Profile.Controllers;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                            NotificationAddressID = 9
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
                            NotificationAddressID = 1
                        },
                        new()
                        {
                            FullAddress = "+4798765432",
                            Address = "98765432",
                            Domain = "+47",
                            AddressType = AddressType.SMS,
                            NotificationAddressID = 2
                        },
                        new()
                        {
                            FullAddress = "+4747765432",
                            Address = "47765432",
                            Domain = "+47",
                            AddressType = AddressType.SMS,
                            NotificationAddressID = 3
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
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNo));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory");
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

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
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory");
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

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

        [Fact]
        public async Task GetMandatoryNotificationAddress_WhenOneAddressFound_ReturnsOk()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNo));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1");
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<NotificationAddressResponse>(responseContent, _serializerOptions);
            Assert.Equal("test@test.com", actual.Email);
        }

        [Fact]
        public async Task GetMandatoryNotificationAddress_WhenNoMatchingOrganization_ReturnsNotFound()
        {
            // Arrange
            var orgNo = "error-org";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1");
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMandatoryNotificationAddress_WhenNoAuth_ReturnsUnautorized()
        {
            // Arrange
            var orgNo = "123456789";

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMandatoryNotificationAddress_WhenHavingWrongAccessToken_ReturnsForbidden()
        {
            // Arrange
            var orgNo = "123456789";

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1");
            string token = PrincipalUtil.GetOrgToken(orgNo);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateMandatory_WhenSuccessWithEmail_ReturnsCreatedResult()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
            .Setup(r => r.CreateNotificationAddressAsync(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
            .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock.Setup(
                c => c.CreateNewNotificationAddress(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("123456789");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "unique@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<NotificationAddressResponse>(responseContent, _serializerOptions);
            Assert.IsType<NotificationAddressResponse>(actual);
        }

        [Fact]
        public async Task CreateMandatory_WhenSuccessWithPhoneNumber_ReturnsCreatedResult()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
            .Setup(r => r.CreateNotificationAddressAsync(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
            .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock.Setup(
                c => c.CreateNewNotificationAddress(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("123456789");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Phone = "91234567", CountryCode = "+47" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<NotificationAddressResponse>(responseContent, _serializerOptions);
            Assert.IsType<NotificationAddressResponse>(actual);
        }

        [Fact]
        public async Task CreateMandatory_WhenAddressAlreadyExists_ReturnsOkResult()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.CreateNotificationAddressAsync(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock.Setup(
                c => c.CreateNewNotificationAddress(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("123456789");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "test@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<NotificationAddressResponse>(responseContent, _serializerOptions);
            Assert.IsType<NotificationAddressResponse>(actual);
        }

        [Theory]
        [InlineData("test")] // Invalid email
        [InlineData("test@.com")] // Invalid email
        [InlineData("test@com")] // Invalid email
        [InlineData("test@com.")] // Invalid email
        [InlineData("test@com@com")] // Invalid email
        [InlineData("test@com..com")] // Invalid email
        public async Task CreateMandatory_WhenWrongFormatOfEmail_ReturnsBadRequest(string email)
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = email };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(responseContent, _serializerOptions);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal(400, actual.Status);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Single(actual.Errors);
            Assert.NotNull(actual.Errors["Email"]);
            Assert.True(actual.Errors.TryGetValue("Email", out var message));
            Assert.Contains("The field Email must match the regular expression", message[0]);
        }

        [Theory]
        [InlineData("invalid", "++47")]
        [InlineData("invalid", "47")]
        [InlineData(" ", "+4700")]
        public async Task CreateMandatory_WhenWrongFormatOfPhone_ReturnsBadRequest(string phone, string countryCode)
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Phone = phone, CountryCode = countryCode };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(responseContent, _serializerOptions);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal(400, actual.Status);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Equal(2, actual.Errors.Count);
            Assert.NotNull(actual.Errors["CountryCode"]);
            Assert.True(actual.Errors.TryGetValue("CountryCode", out var message));
            Assert.Contains("The field CountryCode must match the regular expression", message[0]);

            Assert.NotNull(actual.Errors["Phone"]);
            Assert.True(actual.Errors.TryGetValue("Phone", out var phoneMessage));
            Assert.Contains("The field Phone must match the regular expression", phoneMessage[0]);
        }

        [Fact]
        public async Task UpdateMandatory_WhenSuccessWithEmail_ReturnsUpdatedResult()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.UpdateNotificationAddressAsync(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock
                .Setup(c => c.UpdateNotificationAddress(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("2");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "unique@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<NotificationAddressResponse>(responseContent, _serializerOptions);
            Assert.IsType<NotificationAddressResponse>(actual);
        }

        [Fact]
        public async Task UpdateMandatory_WhenTryingToUpdateToAlreadyExistingAddress_ReturnsConfict()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.UpdateNotificationAddressAsync(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock
                .Setup(c => c.UpdateNotificationAddress(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("2");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "test@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/2")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<ProblemDetails>(responseContent, _serializerOptions);
            Assert.IsType<ProblemDetails>(actual);
            Assert.Equal($"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1", actual.Instance);
        }

        [Fact]
        public async Task UpdateMandatory_WhenNoOrgFound_ReturnsNotFound()
        {
            // Arrange
            var orgNo = "1";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Organization { NotificationAddresses = [], OrganizationNumber = orgNo }]);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "test@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/100")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMandatory_WhenNoAddressFound_ReturnsNotFound()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Organization { NotificationAddresses = [], OrganizationNumber = orgNo }]);
            
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "test@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/100")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("test")] // Invalid email
        [InlineData("test@.com")] // Invalid email
        [InlineData("test@com")] // Invalid email
        [InlineData("test@com.")] // Invalid email
        [InlineData("test@com@com")] // Invalid email
        [InlineData("test@com..com")] // Invalid email
        public async Task UpdateMandatory_WhenWrongFormatOfEmail_ReturnsBadRequest(string email)
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = email };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(responseContent, _serializerOptions);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal(400, actual.Status);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Single(actual.Errors);
            Assert.NotNull(actual.Errors["Email"]);
            Assert.True(actual.Errors.TryGetValue("Email", out var message));
            Assert.Contains("The field Email must match the regular expression", message[0]);
        }

        [Theory]
        [InlineData("invalid", "++47")]
        [InlineData("invalid", "47")]
        [InlineData(" ", "+4700")]
        public async Task UpdateMandatory_WhenWrongFormatOfPhone_ReturnsBadRequest(string phone, string countryCode)
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Phone = phone, CountryCode = countryCode };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<HttpValidationProblemDetails>(responseContent, _serializerOptions);

            Assert.IsType<HttpValidationProblemDetails>(actual);
            Assert.Equal(400, actual.Status);
            Assert.Equal("One or more validation errors occurred.", actual.Title);

            Assert.Equal(2, actual.Errors.Count);
            Assert.NotNull(actual.Errors["CountryCode"]);
            Assert.True(actual.Errors.TryGetValue("CountryCode", out var message));
            Assert.Contains("The field CountryCode must match the regular expression", message[0]);

            Assert.NotNull(actual.Errors["Phone"]);
            Assert.True(actual.Errors.TryGetValue("Phone", out var phoneMessage));
            Assert.Contains("The field Phone must match the regular expression", phoneMessage[0]);
        }

        [Fact]
        public async Task DeleteMandatory_WhenSuccessWithEmail_ReturnsDeletedResult()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
            .Setup(r => r.DeleteNotificationAddressAsync(It.IsAny<int>()))
            .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock
                .Setup(c => c.DeleteNotificationAddress(It.IsAny<string>()))
                .ReturnsAsync("2");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1");
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<NotificationAddressResponse>(responseContent, _serializerOptions);
            Assert.IsType<NotificationAddressResponse>(actual);
        }

        [Fact]
        public async Task DeleteMandatory_WhenTryingToDeleteLastAddress_ReturnsConflict()
        {
            // Arrange
            var orgNo = "987654321";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo));
            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
            .Setup(r => r.DeleteNotificationAddressAsync(It.IsAny<int>()))
            .ReturnsAsync(_testdata.First(o => o.OrganizationNumber == orgNo).NotificationAddresses.First());

            _webApplicationFactorySetup.OrganizationNotificationAddressUpdateClientMock
                .Setup(c => c.DeleteNotificationAddress(It.IsAny<string>()))
                .ReturnsAsync("2");
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/9");
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<ProblemDetails>(responseContent, _serializerOptions);
            Assert.IsType<ProblemDetails>(actual);
        }

        [Fact]
        public async Task DeleteMandatory_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var orgNo = "123456789";
            const int UserId = 2516356;
            Mock<IPDP> pdpMock = GetPDPMockWithResponse("Permit");

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Organization { OrganizationNumber = orgNo, NotificationAddresses = [] }]);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient(pdpMock.Object);

            var input = new NotificationAddressModel { Email = "test@test.com" };
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"/profile/api/v1/organizations/{orgNo}/notificationaddresses/mandatory/1")
            {
                Content = JsonContent.Create(input, options: _serializerOptions)
            };
            httpRequestMessage = CreateAuthorizedRequest(UserId, httpRequestMessage);

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static HttpRequestMessage CreateAuthorizedRequest(int userId, HttpRequestMessage httpRequestMessage)
        {
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }

        private static Mock<IPDP> GetPDPMockWithResponse(string decision)
        {
            var pdpMock = new Mock<IPDP>();
            pdpMock
                .Setup(pdp => pdp.GetDecisionForRequest(It.IsAny<XacmlJsonRequestRoot>()))
                .ReturnsAsync(new XacmlJsonResponse
                {
                    Response =
                    [
                        new XacmlJsonResult
                        {
                            Decision = decision
                        }
                    ]
                });

            return pdpMock;
        }
    }
}
