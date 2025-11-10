using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using Altinn.Authorization.ServiceDefaults.Leases;
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Clients;
using Altinn.Common.PEP.Configuration;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using Altinn.Profile.Authorization;
using Altinn.Profile.Changelog;
using Altinn.Profile.Configuration;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.Utils;
using Altinn.Profile.Health;
using Altinn.Profile.Integrations;
using Altinn.Profile.Integrations.Extensions;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Integrations.Leases;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Altinn.Profile.Middleware;

using AltinnCore.Authentication.JwtCookie;

using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;

using JasperFx.Core;

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

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Swashbuckle.AspNetCore.SwaggerGen;

using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

ConfigureWebHostCreationLogging();

SetConfigurationProviders(builder.Configuration);

ConfigureApplicationLogging(builder.Logging);

ConfigureServices(builder.Services, builder.Configuration);

ConfigureWolverine(builder);

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

void SetConfigurationProviders(ConfigurationManager config)
{
    string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
    config.SetBasePath(basePath);
    config.AddJsonFile(basePath + "altinn-appsettings/altinn-dbsettings-secret.json", optional: true, reloadOnChange: true);

    var keyVaultUri = config.GetValue<string>("kvSetting:SecretUri");

    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        config.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
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

    services.AddOpenTelemetry()
        .ConfigureResource(r =>
            r.AddService(serviceName: Telemetry.AppName, serviceInstanceId: Environment.MachineName))
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddMeter(
                "Microsoft.AspNetCore.Hosting",
                "Microsoft.AspNetCore.Server.Kestrel",
                "System.Net.Http",
                Telemetry.AppName);
        })
        .WithTracing(tracing =>
        {
            tracing.AddSource(Telemetry.AppName);
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddEntityFrameworkCoreInstrumentation();

            tracing.AddProcessor(new RequestFilterProcessor(new HttpContextAccessor()));

            if (builder.Environment.IsDevelopment())
            {
                tracing.SetSampler(new AlwaysOnSampler());
            }
        });

    AddAzureMonitorTelemetryExporters(services, config);

    services.AddSingleton<Telemetry>();

    services
        .AddControllers()
        .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new OptionalJsonConverterFactory());
    });

    services.AddMemoryCache();
    services.AddHealthChecks().AddCheck<HealthCheck>("profile_health_check");

    services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
    services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
    services.Configure<AccessTokenSettings>(config.GetSection("AccessTokenSettings"));
    services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
    services.Configure<AddressMaintenanceSettings>(config.GetSection("AddressMaintenanceSettings"));

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
        .AddPolicy(AuthConstants.OrgNotificationAddress_Write, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn-profil-api-varslingsdaresser-for-virksomheter")))
        .AddPolicy(AuthConstants.UserPartyAccess, policy => policy.Requirements.Add(new PartyAccessRequirement()));

    services.AddScoped<IAuthorizationHandler, OrgResourceAccessHandler>();
    services.AddScoped<IAuthorizationHandler, PartyAccessHandler>();

    services.AddCoreServices(config);
    services.AddRegisterService(config);
    services.AddSblBridgeClients(config);
    services.AddMaskinportenClient(config);
    services.AddProblemDetails();

    services.AddSwaggerGen(swaggerGenOptions => AddSwaggerGen(swaggerGenOptions));

    SetupImportJobs(services, config);
}

static void AddAzureMonitorTelemetryExporters(IServiceCollection services, IConfiguration config)
{
    var instrumentationKey = config.GetValue<string>("ApplicationInsights:InstrumentationKey");

    if (string.IsNullOrEmpty(instrumentationKey))
    {
        return;
    }

    var applicationInsightsConnectionString = string.Format("InstrumentationKey={0}", instrumentationKey);

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
    else
    {
        app.UseExceptionHandler("/profile/api/v1/error");
    }

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

void ConfigureWolverine(WebApplicationBuilder builder)
{
    builder.UseWolverine(opts =>
    {
        var connStr = builder.Configuration.GetDatabaseConnectionString();
        
        // You'll need to independently tell Wolverine where and how to 
        // store messages as part of the transactional inbox/outbox
        opts.PersistMessagesWithPostgresql(connStr);

        // Adding EF Core transactional middleware, saga support,
        // and EF Core support for Wolverine storage operations
        opts.UseEntityFrameworkCoreTransactions();

        opts.Policies.UseDurableLocalQueues();

        opts.Discovery.IncludeAssembly(typeof(FavoriteAddedEventHandler).Assembly);

        opts
            .OnException<InternalServerErrorException>()
            .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    });
}

void SetupImportJobs(IServiceCollection services, IConfiguration config)
{
    services.AddHttpClient<IChangeLogClient, ChangeLogClient>();
    services.AddScoped<IChangelogSyncMetadataRepository, ChangelogSyncMetadataRepository>();
    services.AddSingleton<ILeaseProvider, PostgresqlLeaseProvider>();
    services.AddSingleton<ILeaseRepository, LeaseRepository>();
    services.AddLeaseManager();

    if (config.GetValue<bool>("ImportJobSettings:FavoritesImportEnabled"))
    {
        services.AddScoped<IFavoriteSyncRepository, FavoriteSyncRepository>();

        services.AddRecurringJob<FavoriteImportJob>(settings =>
        {
            settings.LeaseName = LeaseNames.A2FavoriteImport;
            settings.Interval = TimeSpan.FromMinutes(1);
        });
    }

    if (config.GetValue<bool>("ImportJobSettings:NotificationSettingsImportEnabled"))
    {
        services.AddRecurringJob<NotificationSettingImportJob>(settings =>
        {
            settings.LeaseName = LeaseNames.A2NotificationSettingImport;
            settings.Interval = TimeSpan.FromMinutes(1);
        });
    }

    if (config.GetValue<bool>("ImportJobSettings:ProfileSettingsImportEnabled"))
    {
        services.AddRecurringJob<ProfileSettingImportJob>(settings =>
        {
            settings.LeaseName = LeaseNames.A2ProfileSettingImport;
            settings.Interval = TimeSpan.FromMinutes(1);
        });
    }
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
