using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
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
using Altinn.Profile.Core;
using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.Utils;
using Altinn.Profile.Extensions;
using Altinn.Profile.Health;
using Altinn.Profile.Integrations;
using Altinn.Profile.Integrations.Extensions;
using Altinn.Profile.Middleware;

using AltinnCore.Authentication.JwtCookie;

using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;

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
using Microsoft.OpenApi;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Swashbuckle.AspNetCore.SwaggerGen;

ILogger logger;

var builder = WebApplication.CreateBuilder(args);

ConfigureWebHostCreationLogging();

SetConfigurationProviders(builder.Configuration);

ConfigureApplicationLogging(builder.Logging);

ConfigureServices(builder.Services, builder.Configuration);

WebApplication app = builder.Build();

if (args.Contains("--run-db-migrations"))
{
    // Migration mode: runs with admin database access
    // For Kubernetes pre-upgrade hooks
    await Migrate();
    return;
}

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
    services.Configure<PortalAccessSettings>(config.GetSection("PortalAccessSettings"));

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
        .AddPolicy(AuthConstants.SupportDashboardAccess, policy => policy.Requirements.Add(new ScopeAccessRequirement("altinn:profile.support.admin")))
        .AddPolicy(AuthConstants.OrgNotificationAddress_Read, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn-profil-api-varslingsdaresser-for-virksomheter")))
        .AddPolicy(AuthConstants.OrgNotificationAddress_Write, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn-profil-api-varslingsdaresser-for-virksomheter")))
        .AddPolicy(AuthConstants.UserPartyAccess, policy => policy.Requirements.Add(new PartyAccessRequirement()))
        .AddPolicy(AuthConstants.PortalEndUserAccess, policy => policy.Requirements.Add(new FeatureToggledScopeAccessRequirement("altinn:portal/enduser")))
        .AddPolicy(AuthConstants.ScopeEnduserOrNotificationSettingsRead, policy => policy.Requirements.Add(new ScopeAccessRequirement(["altinn:portal/enduser", "altinn:profile/enduser:notificationsettings.read"])));

    services.AddScoped<IAuthorizationHandler, OrgResourceAccessHandler>();
    services.AddScoped<IAuthorizationHandler, PartyAccessHandler>();
    services.AddScoped<IAuthorizationHandler, ScopeAccessHandler>();
    services.AddScoped<IAuthorizationHandler, FeatureToggledScopeAccessHandler>();

    services.AddCoreServices(config);
    services.AddRegisterService(config);
    services.AddMaskinportenClient(config);
    services.AddProblemDetails();

    services.AddSwaggerGen(swaggerGenOptions => AddSwaggerGen(swaggerGenOptions));

    services.SetupLeasing();
    services.AddSyncJobs(config);
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

    if (app.Environment.IsDevelopment())
    {
        logger.LogInformation("IsDevelopment");

        // Enable higher level of detail in exceptions related to JWT validation
        IdentityModelEventSource.ShowPII = true;

        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else if (app.Environment.IsStaging())
    {
        logger.LogInformation("IsStaging");

        // Enable higher level of detail in exceptions related to JWT validation
        IdentityModelEventSource.ShowPII = true;
    }

    app.UseExceptionHandler("/profile/api/v1/error");

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
}

async Task Migrate()
{
    try
    {
        await app.RunDatabaseMigrationsAsync(builder.Configuration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed");
        Environment.Exit(1);
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
