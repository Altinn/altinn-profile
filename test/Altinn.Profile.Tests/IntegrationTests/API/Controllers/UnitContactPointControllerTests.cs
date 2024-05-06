using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Profile.Controllers;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Integrations.SblBridge;
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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public UnitContactPointControllerTests(WebApplicationFactory<UnitContactPointController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<UnitContactPointController>(factory)
            {
                SblBridgeHttpMessageHandler = new DelegatingHandlerStub(async (request, token) =>
                {
                    string orgNo = await request.Content.ReadAsStringAsync(token);
                    return GetSBlResponseFromSBL(orgNo);
                })
            };

            SblBridgeSettings sblBrideSettings = new() { ApiProfileEndpoint = "http://localhost/" };
            _webApplicationFactorySetup.SblBridgeSettingsOptions.Setup(s => s.Value).Returns(sblBrideSettings);
        }

        [Fact]
        public async Task PostLookup_SuccessResult_ReturnsOk()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["123456789"]
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<UnitContactPointsList>(responseContent, _serializerOptions);
            Assert.Empty(actual.ContactPointsList);
        }

        [Fact]
        public async Task PostLookup_ErrorResult_ReturnsProblemDetails()
        {
            // Arrange
            UnitContactPointLookup input = new()
            {
                OrganizationNumbers = ["error-org"]
            };

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/profile/api/v1/units/contactpoint")
            {
                Content = new StringContent(JsonSerializer.Serialize(input, _serializerOptions), System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private HttpResponseMessage GetSBlResponseFromSBL(string orgNo)
        {
            switch (orgNo)
            {
                case "\"123456789\"":
                    UnitContactPointsList contactPoints = new()
                    {
                        ContactPointsList = [new UnitContactPoints
                        {
                            OrganizationNumber = "123456789",
                            UserContactPoints = [new UserContactPoints()
                            {
                                NationalIdentityNumber = "16069412345"
                            }
                            ]
                        }
                        ]
                    };

                    return new HttpResponseMessage() { Content = JsonContent.Create(contactPoints, options: _serializerOptions), StatusCode = HttpStatusCode.OK };
                default:
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable };
            }
        }
    }
}
