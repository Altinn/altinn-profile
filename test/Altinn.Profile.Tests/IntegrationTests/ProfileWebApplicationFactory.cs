#nullable enable

using System.IO;

using Altinn.Common.AccessToken.Services;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Mocks.Authentication;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Wolverine;

namespace Altinn.Profile.Tests.IntegrationTests;

public class ProfileWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.test.json");
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

            services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
            services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();
        });
    }
}
