using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Clients;
using Altinn.Common.PEP.Configuration;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using Altinn.Profile.Authorization;
using Altinn.Profile.Configuration;
using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Health;
using Altinn.Profile.Integrations;
using Altinn.Profile.Integrations.Extensions;
using Altinn.Profile.Mappers;
using Altinn.Profile.Telemetry;
using AltinnCore.Authentication.JwtCookie;

using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Security.KeyVault.Secrets;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Npgsql;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Swashbuckle.AspNetCore.SwaggerGen;

ILogger logger;

const string VaultApplicationInsightsKey = "ApplicationInsights--InstrumentationKey";

string applicationInsightsConnectionString = string.Empty;

var builder = WebApplication.CreateBuilder(args);

ConfigureWebHostCreationLogging();

await SetConfigurationProviders(builder.Configuration);

ConfigureApplicationLogging(builder.Logging);

ConfigureServices(builder.Services, builder.Configuration);

WebApplication app = builder.Build();

app.SetUpPostgreSql(builder.Configuration);

Configure();

await app.RunAsync();

void ConfigureWebHostCreationLogging()
{
    var logFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Altinn.Profile.Program", LogLevel.Debug)
            .AddConsole();
    });

    logger = logFactory.CreateLogger<Program>();
}

async Task SetConfigurationProviders(ConfigurationManager config)
{
    string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
    config.SetBasePath(basePath);
    config.AddJsonFile(basePath + "altinn-appsettings/altinn-dbsettings-secret.json", optional: true, reloadOnChange: true);

    await ConnectToKeyVaultAndSetApplicationInsights(config);
}

async Task ConnectToKeyVaultAndSetApplicationInsights(ConfigurationManager config)
{
    KeyVaultSettings keyVaultSettings = new();
    config.GetSection("kvSetting").Bind(keyVaultSettings);
    if (!string.IsNullOrEmpty(keyVaultSettings.SecretUri))
    {
        logger.LogInformation("Program // Set app insights connection string // App");

        DefaultAzureCredential azureCredentials = new();

        SecretClient client = new(new Uri(keyVaultSettings.SecretUri), azureCredentials);

        config.AddAzureKeyVault(new Uri(keyVaultSettings.SecretUri), azureCredentials);

        try
        {
            KeyVaultSecret keyVaultSecret = await client.GetSecretAsync(VaultApplicationInsightsKey);
            applicationInsightsConnectionString = string.Format("InstrumentationKey={0}", keyVaultSecret.Value);
        }
        catch (Exception vaultException)
        {
            logger.LogError(vaultException, "Unable to read application insights key.");
        }
    }
}

void ConfigureApplicationLogging(ILoggingBuilder logging)
{
    logging.AddOpenTelemetry(builder =>
    {
        builder.IncludeFormattedMessage = true;
        builder.IncludeScopes = true;
    });
}

void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    logger.LogInformation("Program // ConfigureServices");

    var attributes = new List<KeyValuePair<string, object>>(2)
    {
        KeyValuePair.Create("service.name", (object)"platform-profile"),
    };

    services.AddOpenTelemetry()
        .ConfigureResource(resourceBuilder => resourceBuilder.AddAttributes(attributes))
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddMeter(
                "Microsoft.AspNetCore.Hosting",
                "Microsoft.AspNetCore.Server.Kestrel",
                "System.Net.Http");
        })
        .WithTracing(tracing =>
        {
            if (builder.Environment.IsDevelopment())
            {
                tracing.SetSampler(new AlwaysOnSampler());
            }

            tracing.AddAspNetCoreInstrumentation();
            tracing.AddProcessor(new RequestFilterProcessor(new HttpContextAccessor()));

            tracing.AddHttpClientInstrumentation();
            tracing.AddEntityFrameworkCoreInstrumentation();

            if (builder.Environment.IsDevelopment())
            {
                tracing.AddConsoleExporter();
            }
        });

    if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
    {
        AddAzureMonitorTelemetryExporters(services, applicationInsightsConnectionString);
    }

    services.AddControllers();

    services.AddMemoryCache();
    services.AddHealthChecks().AddCheck<HealthCheck>("profile_health_check");

    services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
    services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
    services.Configure<AccessTokenSettings>(config.GetSection("AccessTokenSettings"));
    services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));

    services.AddSingleton(config);
    services.AddSingleton<IAuthorizationHandler, AccessTokenHandler>();
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProvider>();

    services.AddHttpClient<AuthorizationApiClient>();
    services.AddSingleton<IPDP, PDPAppSI>();

    services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
        .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
        {
            GeneralSettings generalSettings = config.GetSection("GeneralSettings").Get<GeneralSettings>();
            options.JwtCookieName = generalSettings.JwtCookieName;
            options.MetadataAddress = generalSettings.OpenIdWellKnownEndpoint;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

    services.AddAuthorizationBuilder()
        .AddPolicy(AuthConstants.PlatformAccess, policy => policy.Requirements.Add(new AccessTokenRequirement()))
        .AddPolicy(AuthConstants.OrgNotificationAddress_Read, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn-profil-api-varslingsdaresser-for-virksomheter")))
        .AddPolicy(AuthConstants.OrgNotificationAddress_Write, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn-profil-api-varslingsdaresser-for-virksomheter")));

    services.AddScoped<IAuthorizationHandler, OrgResourceAccessHandler>();

    services.AddCoreServices(config);
    services.AddRegisterService(config);
    services.AddSblBridgeClients(config);
    services.AddMaskinportenClient(config);
    services.AddProblemDetails();

    services.AddSwaggerGen(swaggerGenOptions => AddSwaggerGen(swaggerGenOptions));
}

static void AddAzureMonitorTelemetryExporters(IServiceCollection services, string applicationInsightsConnectionString)
{
    services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddAzureMonitorLogExporter(o =>
    {
        o.ConnectionString = applicationInsightsConnectionString;
    }));
    services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddAzureMonitorMetricExporter(o =>
    {
        o.ConnectionString = applicationInsightsConnectionString;
    }));
    services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddAzureMonitorTraceExporter(o =>
    {
        o.ConnectionString = applicationInsightsConnectionString;
    }));
}

void AddSwaggerGen(SwaggerGenOptions swaggerGenOptions)
{
    swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "Altinn Profile", Version = "v1" });

    try
    {
        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        swaggerGenOptions.IncludeXmlComments(xmlPath);
    }
    catch (Exception e)
    {
        logger.LogWarning(e, "Program // Exception when attempting to include the XML comments file.");
    }
}

void Configure()
{
    logger.LogInformation("Program // Configure {AppName}", app.Environment.ApplicationName);

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        logger.LogInformation("IsDevelopment || IsStaging");

        // Enable higher level of detail in exceptions related to JWT validation
        IdentityModelEventSource.ShowPII = true;

        app.UseExceptionHandler("/profile/api/v1/error");
    }
    
    app.UseExceptionHandler("/profile/api/v1/error");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
}

/// <summary>
/// Startup class.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed partial class Program
{
    private Program()
    {
    }
}
