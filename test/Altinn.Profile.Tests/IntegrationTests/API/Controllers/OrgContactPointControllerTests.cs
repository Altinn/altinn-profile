using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Core.OrganizationNotificationAddresses;

using Altinn.Profile.Tests.IntegrationTests.Mocks;
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
            _testdata =
            [
                new()
                {
                    RegistryOrganizationId = 1,
                    RegistryOrganizationNumber = "123456789",
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
        public async Task PostLookup_SuccessResult_ReturnsOk()
        {
            // Arrange
            OrgContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789"],
            };

            _webApplicationFactorySetup.OrganizationNotificationAddressRepositoryMock
                .Setup(r => r.GetOrganizationsAsync(It.IsAny<OrgContactPointLookup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata);
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
        public async Task PostLookup_ErrorResult_ReturnsProblemDetails()
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
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
