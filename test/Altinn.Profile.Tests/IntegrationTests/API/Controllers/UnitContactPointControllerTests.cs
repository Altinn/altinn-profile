using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class UnitContactPointControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly ProfileWebApplicationFactory<Program> _factory;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public UnitContactPointControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;

            _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
            {
                string requestString = await request.Content.ReadAsStringAsync(token);
                UnitContactPointLookup lookup = JsonSerializer.Deserialize<UnitContactPointLookup>(requestString, _serializerOptions);
                return GetSBlResponseFromSBL(lookup.OrganizationNumbers[0]);
            });
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

            HttpClient client = _factory.CreateClient();
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

            HttpClient client = _factory.CreateClient();
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
        [InlineData("not deserialiable to input model")]
        [InlineData("{\"organizationNumbers\":[null],\"resourceId\":null}")]
        [InlineData("{\"organizationNumbers\":null,\"resourceId\":\"resurs\"}")]
        public async Task PostLookup_InvalidInputValues_ReturnsBadRequest(string input)
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint/lookup")
            {
                Content = new StringContent(input, System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            var responseContent = await response.Content.ReadAsStringAsync();

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
                            ContactPoints = [new Integrations.SblBridge.Unit.Profile.SblUserRegisteredContactPoint()
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
