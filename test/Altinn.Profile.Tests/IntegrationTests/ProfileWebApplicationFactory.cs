#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Integrations.Authorization;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;
using Altinn.Profile.Integrations.SblBridge.User.Profile;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Mocks.Authentication;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Wolverine;

namespace Altinn.Profile.Tests.IntegrationTests;

public sealed class ProfileWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly static TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(10);

    public Mock<IAuthorizationClient> AuthorizationClientMock { get; set; } = new();

    public Mock<IContactRegisterHttpClient> ContactRegisterServiceMock { get; set; } = new();

    public Mock<IUnitContactPointsService>? UnitContactPointsServiceMock { get; set; }

    public Mock<IPersonService> PersonServiceMock { get; set; } = new();

    public Mock<IOrganizationNotificationAddressRepository> OrganizationNotificationAddressRepositoryMock { get; set; } = new();

    public Mock<IOrganizationNotificationAddressSyncClient> OrganizationNotificationAddressSyncClientMock { get; set; } = new();

    public Mock<IOrganizationNotificationAddressUpdateClient> OrganizationNotificationAddressUpdateClientMock { get; set; } = new();

    public Mock<IPartyGroupRepository> PartyGroupRepositoryMock { get; set; } = new();

    public Mock<IPDP>? PdpMock { get; set; }

    public Mock<IProfessionalNotificationsRepository> ProfessionalNotificationsRepositoryMock { get; set; } = new();

    public Mock<IRegisterClient> RegisterClientMock { get; set; } = new();

    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> MessageHandlerFunc { get; set; } =
        (request, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

    public DelegatingHandlerStub SblBridgeHttpMessageHandler { get; set; } = new();

    public Mock<IOptions<SblBridgeSettings>> SblBridgeSettingsOptions { get; set; } = new();

    public Mock<ILogger<UnitProfileClient>> UnitProfileClientLogger { get; set; } = new();

    public Mock<ILogger<UserProfileClient>> UserProfileClientLogger { get; set; } = new();

    public Mock<IProfileSettingsRepository> ProfileSettingsRepositoryMock { get; set; } = new();

    public Dictionary<string, string?> InMemoryConfigurationCollection { get; set; } = new();

    public MemoryCache MemoryCache { get; set; } = new(new MemoryCacheOptions());

    public ProfileWebApplicationFactory()
    {
        SblBridgeSettingsOptions.Setup(gso => gso.Value).Returns(
            new SblBridgeSettings
            {
                ApiProfileEndpoint = "https://at22.altinn.cloud/sblbridge/profile/api/"
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.test.json");
            config.AddInMemoryCollection(InMemoryConfigurationCollection);
        });

        builder.ConfigureServices(services =>
        {
            // Override the Wolverine configuration in the application
            // to run the application in "solo" mode for faster
            // testing cold starts
            services.RunWolverineInSoloMode();

            // And just for completion, disable all Wolverine external 
            // messaging transports
            services.DisableAllExternalWolverineTransports();

            // Replace services with mocks or stubs
            services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
            services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();
            services.AddSingleton<IMemoryCache>(MemoryCache);

            if (PdpMock is not null)
            {
                services.AddSingleton(PdpMock.Object);
            }
            else
            {
                services.AddSingleton<IPDP, PepWithPDPAuthorizationMockSI>();
            }

            services.AddSingleton(AuthorizationClientMock.Object);
            services.AddSingleton(ContactRegisterServiceMock.Object);

            if (UnitContactPointsServiceMock is not null)
            {
                // Most tests will not need to mock UnitContactPointsService, so only add it if explicitly set.
                services.AddSingleton(UnitContactPointsServiceMock.Object);
            }

            services.AddSingleton(PersonServiceMock.Object);
            services.AddSingleton(OrganizationNotificationAddressRepositoryMock.Object);
            services.AddSingleton(OrganizationNotificationAddressSyncClientMock.Object);
            services.AddSingleton(OrganizationNotificationAddressUpdateClientMock.Object);
            services.AddSingleton(PartyGroupRepositoryMock.Object);
            services.AddSingleton(ProfessionalNotificationsRepositoryMock.Object);
            services.AddSingleton(RegisterClientMock.Object);
            services.AddSingleton(ProfileSettingsRepositoryMock.Object);
            services.AddSingleton(sp =>
            {
                var altinnConfig = new AddressMaintenanceSettings
                {
                    ValidationReminderDays = 90,
                    IgnoreUnitProfileConfirmationDays = 365
                };
                var optionsMock = new Mock<IOptions<AddressMaintenanceSettings>>();
                optionsMock.Setup(o => o.Value).Returns(altinnConfig);
                return optionsMock.Object;
            });

            // Using the real/actual implementations, but with a mocked message handler.
            // Haven't found any other ways of injecting a mocked message handler to simulate SBL Bridge.
            services.AddSingleton<IUserProfileClient>(
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
    }

    /*****
     * DisposeAsync is overridden to ensure that the application is stopped gracefully.
     * Running the tests without this can lead to issues with Wolverine and disposal. 
     * Still not entierly clear why this is needed.
     * https://github.com/dotnet/aspnetcore/issues/40271#issuecomment-2481337081
     */

    public override async ValueTask DisposeAsync()
    {
        await StopApplication().ConfigureAwait(false);
        await WaitForDisposal().ConfigureAwait(false);

        foreach (var factory in Factories)
        {
            await factory.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task StopApplication(CancellationToken forcefulStoppingToken = default)
    {
        try
        {
            var tcs = new TaskCompletionSource();
            var lifetime = Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopped.Register(() => tcs.TrySetResult());
            lifetime.StopApplication();
            await tcs.Task.WaitAsync(ShutdownTimeout, forcefulStoppingToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
    }

    private async Task WaitForDisposal(CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // IHostApplicationLifetime.ApplicationStopped is triggered before the host (and its service collection)
                // is disposed, so additionally wait until the service collection is disposed for a clean shutdown.
                _ = Services.GetRequiredService<IHostApplicationLifetime>();
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
        }
    }
}
