using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class UnitContactPointControllerTests : IClassFixture<WebApplicationFactory<UnitContactPointController>>
    {
        private readonly WebApplicationFactorySetup<UnitContactPointController> _webApplicationFactorySetup;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public UnitContactPointControllerTests(WebApplicationFactory<UnitContactPointController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<UnitContactPointController>(factory);

            _webApplicationFactorySetup.SblBridgeHttpMessageHandler = new DelegatingHandlerStub(async (request, token) =>
                {
                    string requestString = await request.Content.ReadAsStringAsync(token);
                    UnitContactPointLookup lookup = JsonSerializer.Deserialize<UnitContactPointLookup>(requestString, _serializerOptions);
                    return GetSBlResponseFromSBL(lookup.OrganizationNumbers[0]);
                });

            SblBridgeSettings sblBrideSettings = new() { ApiProfileEndpoint = "http://localhost/" };
            _webApplicationFactorySetup.SblBridgeSettingsOptions.Setup(s => s.Value).Returns(sblBrideSettings);
        }

        [Fact]
        public async Task PostLookup_SuccessResult_ReturnsOk()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789"],
                ResourceId = "app_ttd_apps-test"
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Single(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_ErrorResult_ReturnsProblemDetails()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["error-org"],
                ResourceId = "app_ttd_apps-test"
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Theory]
        [InlineData("not json")]
        public async Task PostLookup_InvalidInputValues_ReturnsBadRequest(string input)
        {
            // Arrange
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private HttpResponseMessage GetSBlResponseFromSBL(string orgNo)
        {
            switch (orgNo)
            {
                case "123456789":
                    var input = new List<PartyNotificationContactPoints>()
                    {
                        new PartyNotificationContactPoints()
                        {
                            ContactPoints = [new UserRegisteredContactPoint()
                            {
                                LegacyUserId = 20001,
                                Email = "user@email.com"
                            }
                             ],
                            LegacyPartyId = 50001,
                            OrganizationNumber = "123456789",
                            PartyId = Guid.NewGuid()
                        }
                    };

                    return new HttpResponseMessage() { Content = JsonContent.Create(input, options: _serializerOptions), StatusCode = HttpStatusCode.OK };
                default:
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable };
            }
        }
    }
}
