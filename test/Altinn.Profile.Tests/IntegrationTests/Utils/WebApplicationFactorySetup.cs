using System.IO;
using System.Net.Http;

using Altinn.Common.AccessToken.Services;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;
using Altinn.Profile.Integrations.SblBridge.User.Profile;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Mocks.Authentication;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace Altinn.Profile.Tests.IntegrationTests.Utils;

public class WebApplicationFactorySetup<T>
    where T : class
{
    private readonly WebApplicationFactory<T> _webApplicationFactory;

    public WebApplicationFactorySetup(WebApplicationFactory<T> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    public Mock<IContactRegisterHttpClient> ContactRegisterServiceMock { get; set; } = new();

    public Mock<ILogger<UserProfileClient>> UserProfileClientLogger { get; set; } = new();

    public Mock<IOrganizationNotificationAddressHttpClient> OrganizationNotificationAddressServiceMock { get; set; } = new();

    public Mock<ILogger<UnitProfileClient>> UnitProfileClientLogger { get; set; } = new();

    public Mock<IOptions<SblBridgeSettings>> SblBridgeSettingsOptions { get; set; } = new();

    public HttpMessageHandler SblBridgeHttpMessageHandler { get; set; } = new DelegatingHandlerStub();

    public HttpClient GetTestServerClient()
    {
        MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

        SblBridgeSettingsOptions.Setup(gso => gso.Value).Returns(
            new SblBridgeSettings
            {
                ApiProfileEndpoint = "https://at22.altinn.cloud/sblbridge/profile/api/"
            });

        return _webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.test.json");
            });
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();
                services.AddSingleton<IMemoryCache>(memoryCache);

                // Using a mock to stop tests from calling the contact register service.
                services.AddSingleton(ContactRegisterServiceMock.Object);

                services.AddSingleton(OrganizationNotificationAddressServiceMock.Object);

                // Using the real/actual implementation of IUserProfileService, but with a mocked message handler.
                // Haven't found any other ways of injecting a mocked message handler to simulate SBL Bridge.
                services.AddSingleton<IUserProfileRepository>(
                    new UserProfileClient(
                        new HttpClient(SblBridgeHttpMessageHandler),
                        UserProfileClientLogger.Object,
                        SblBridgeSettingsOptions.Object));

                services.AddSingleton<IUnitProfileRepository>(
                    new UnitProfileClient(
                       new HttpClient(SblBridgeHttpMessageHandler),
                       UnitProfileClientLogger.Object,
                       SblBridgeSettingsOptions.Object));
            });
        }).CreateClient();
    }
}
