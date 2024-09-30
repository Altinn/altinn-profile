using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Tests.IntegrationTests.Mocks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge
{
    public class UnitProfileRepositoryTest
    {
        [Fact]
        public async Task GetUserRegisteredContactPoints_BridgeRespondsWithOk_UnitContactPointsListReturned()
        {
            // Arrange
            UnitContactPointLookup lookup = new()
            {
                OrganizationNumbers = ["123456789"],
                ResourceId = "app_ttd_apps-test"
            };

            var sblBridgeHttpMessageHandler = new DelegatingHandlerStub(async (request, token) =>
            {
                if (request!.RequestUri!.AbsolutePath.EndsWith("units/contactpointslookup"))
                {
                    var contentData = new List<PartyNotificationContactPoints>();
                    JsonContent content = JsonContent.Create(contentData);

                    return await Task.FromResult(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = content
                    });
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            SblBridgeSettings settings = new()
            {
                ApiProfileEndpoint = "https://platform.at22.altinn.cloud/profile/api/v1/"
            };

            var sut = new UnitProfileRepository(
                                    new HttpClient(sblBridgeHttpMessageHandler),
                                    Mock.Of<ILogger<UnitProfileRepository>>(),
                                    Options.Create(settings));

            // Act
            Result<UnitContactPointsList, bool> result = await sut.GetUserRegisteredContactPoints(lookup);

            // Assert            
            result.Match(
                success =>
                {
                    Assert.IsType<UnitContactPointsList>(success);
                },
                error => throw new Exception("No error value should be returned if SBL client respons with 200 OK."))
;
        }

        [Fact]
        public async Task GetUserRegisteredContactPoints_BridgeRespondsWithServiceUnavailable_ErrorReturned_UnitContactPointsListReturned()
        {
            // Arrange
            UnitContactPointLookup lookup = new()
            {
                OrganizationNumbers = ["123456789"],
                ResourceId = "app_ttd_apps-test"
            };

            var sblBridgeHttpMessageHandler = new DelegatingHandlerStub((request, token) =>
            {
                if (request!.RequestUri!.AbsolutePath.EndsWith("units/contactpointslookup"))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

            SblBridgeSettings settings = new()
            {
                ApiProfileEndpoint = "https://platform.at22.altinn.cloud/profile/api/v1/"
            };

            var sut = new UnitProfileRepository(
                                    new HttpClient(sblBridgeHttpMessageHandler),
                                    Mock.Of<ILogger<UnitProfileRepository>>(),
                                    Options.Create(settings));

            // Act
            Result<UnitContactPointsList, bool> result = await sut.GetUserRegisteredContactPoints(lookup);

            // Assert            
            result.Match(
                success => throw new Exception("No success value should be returned if SBL client respons with 5xx."),
                error =>
                {
                    Assert.IsType<bool>(error);
                    Assert.False(error);
                })
;
        }
    }
}
