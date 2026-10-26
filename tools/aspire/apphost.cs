#:sdk Aspire.AppHost.Sdk@13.6.1
#:property AspireUseCliBundle=true

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string ProfileProject = "../../src/Altinn.Profile/Altinn.Profile.csproj";

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile(Path.Combine(builder.AppHostDirectory, "appsettings.json"), optional: true)
    .AddJsonFile(Path.Combine(builder.AppHostDirectory, "appsettings.Local.json"), optional: true);

// No launch profiles: launchSettings.json hardcodes ASPNETCORE_URLS, which collides with Aspire's.
var migrations = builder.AddProject("migrations", ProfileProject, launchProfileName: null)
    .WithArgs("--run-db-migrations")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Fixed, unproxied port so the URLs used by Bruno and other local services keep working.
var profile = builder.AddProject("profile", ProfileProject, launchProfileName: null)
    .WithHttpEndpoint(port: 5030, targetPort: 5030, isProxied: false)
    .WithUrl("http://localhost:5030/swagger", "Swagger")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WaitForCompletion(migrations);

// The container is started outside Aspire so it keeps running when it stops; the resource only tails its logs.
if (builder.Configuration.GetValue("Postgres:UsePodman", true))
{
    if (RunPodman("profile-db", "compose", "up", "-d") != 0)
    {
        throw new InvalidOperationException("podman compose up failed for profile-db");
    }

    WaitForPostgres();
    var postgres = builder.AddExecutable("postgres", "podman", ".", "logs", "-f", "profile-db");
    migrations.WaitFor(postgres).WithParentRelationship(postgres);
}

AddClusterSwitch(profile, "altinn-profile", 5030);
AddTests("tests", profile, "../../Altinn.Profile.sln");

var app = builder.Build();

HashSet<string> pendingRestarts = [];
using var watcher = WatchSource(app.Services, "../../src", profile);

app.Run();

int RunPodman(string workingDirectory, params string[] podmanArgs)
{
    var startInfo = new ProcessStartInfo("podman", podmanArgs) { WorkingDirectory = Path.Combine(builder.AppHostDirectory, workingDirectory) };
    using var podman = Process.Start(startInfo)!;
    podman.WaitForExit();
    return podman.ExitCode;
}

// A new container runs its init scripts on a socket-only server first, so wait until Postgres accepts TCP connections.
void WaitForPostgres()
{
    for (int attempt = 0; attempt < 30; attempt++)
    {
        if (RunPodman(".", "exec", "profile-db", "pg_isready", "-h", "localhost", "-U", "platform_profile_admin", "-d", "profiledb") == 0)
        {
            return;
        }

        Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    throw new InvalidOperationException("profile-db did not become ready within 30 seconds");
}

void AddClusterSwitch(IResourceBuilder<ProjectResource> service, string clusterService, int port)
{
    Process? forward = null;

    service.WithCommand(
        "use-cluster",
        "Use cluster",
        async context =>
        {
            await context.Services.GetRequiredService<ResourceCommandService>().ExecuteCommandAsync(service.Resource, KnownResourceCommands.StopCommand, context.CancellationToken);

            var logger = context.Services.GetRequiredService<ResourceLoggerService>().GetLogger(service.Resource);
            forward = StartPortForward(clusterService, port, logger);
            var started = forward;
            context.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() => started.Kill());

            await context.Services.GetRequiredService<ResourceNotificationService>().PublishUpdateAsync(service.Resource, s => s with { State = new ResourceStateSnapshot("Cluster", KnownResourceStateStyles.Info) });
            return CommandResults.Success();
        },
        new CommandOptions { IconName = "Cloud", UpdateState = _ => forward is null ? ResourceCommandState.Enabled : ResourceCommandState.Hidden });

    service.WithCommand(
        "use-local",
        "Use local",
        async context =>
        {
            forward?.Kill();
            forward = null;
            return await context.Services.GetRequiredService<ResourceCommandService>().ExecuteCommandAsync(service.Resource, KnownResourceCommands.StartCommand, context.CancellationToken);
        },
        new CommandOptions { IconName = "Desktop", UpdateState = _ => forward is null ? ResourceCommandState.Hidden : ResourceCommandState.Enabled });
}

static Process StartPortForward(string clusterService, int port, ILogger logger)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo("kubectl", ["port-forward", $"service/{clusterService}", $"{port}:80"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        },
        EnableRaisingEvents = true,
    };

    process.OutputDataReceived += (_, e) =>
    {
        if (e.Data is not null)
        {
            logger.LogInformation("{Line}", e.Data);
        }
    };
    process.ErrorDataReceived += (_, e) =>
    {
        if (e.Data is not null)
        {
            logger.LogError("{Line}", e.Data);
        }
    };
    process.Exited += (_, _) => logger.LogWarning("kubectl port-forward exited with code {ExitCode}", process.ExitCode);

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    return process;
}

void AddTests(string name, IResourceBuilder<ProjectResource> service, string solution)
{
    var tests = builder.AddExecutable(name, "dotnet", ".", "test", solution)
        .WithExplicitStart()
        .WithParentRelationship(service.Resource);

    builder.Eventing.Subscribe<BeforeStartEvent>(async (e, ct) =>
    {
        var notifications = e.Services.GetRequiredService<ResourceNotificationService>();
        await notifications.PublishUpdateAsync(tests.Resource, s => s with { IsHidden = true });

        var stopping = e.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
        _ = ShowTestResultAsync(notifications, tests.Resource, stopping);
    });

    service.WithCommand(
        "run-tests",
        "Run tests",
        async context =>
        {
            await context.Services.GetRequiredService<ResourceNotificationService>().PublishUpdateAsync(tests.Resource, s => s with { IsHidden = false });
            return await context.Services.GetRequiredService<ResourceCommandService>().ExecuteCommandAsync(tests.Resource, KnownResourceCommands.StartCommand, context.CancellationToken);
        },
        new CommandOptions { IconName = "Beaker", IsHighlighted = false });
}

FileSystemWatcher WatchSource(IServiceProvider services, string folder, IResourceBuilder<ProjectResource> service)
{
    string[] extensions = [".cs", ".csproj", ".json"];
    var watcher = new FileSystemWatcher(Path.Combine(builder.AppHostDirectory, folder)) { IncludeSubdirectories = true };

    FileSystemEventHandler onChange = (_, e) =>
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        if (!extensions.Contains(Path.GetExtension(e.FullPath)) || e.FullPath.Contains($"{separator}bin{separator}") || e.FullPath.Contains($"{separator}obj{separator}"))
        {
            return;
        }

        _ = NotifyChangedAsync(services, service.Resource);
    };

    watcher.Changed += onChange;
    watcher.Created += onChange;
    watcher.Deleted += onChange;
    watcher.Renamed += (sender, e) => onChange(sender, e);
    watcher.EnableRaisingEvents = true;
    return watcher;
}

async Task NotifyChangedAsync(IServiceProvider services, ProjectResource service)
{
    lock (pendingRestarts)
    {
        if (!pendingRestarts.Add(service.Name))
        {
            return;
        }
    }

    await Task.Delay(TimeSpan.FromSeconds(2));

    var result = await services.GetRequiredService<IInteractionService>().PromptNotificationAsync(
        $"{service.Name} has changed",
        "Restart it to pick up the changes.",
        new NotificationInteractionOptions { Intent = MessageIntent.Warning, PrimaryButtonText = "Restart" });

    lock (pendingRestarts)
    {
        pendingRestarts.Remove(service.Name);
    }

    if (!result.Canceled && result.Data)
    {
        await services.GetRequiredService<ResourceCommandService>().ExecuteCommandAsync(service, KnownResourceCommands.RestartCommand);
    }
}

async Task ShowTestResultAsync(ResourceNotificationService notifications, IResource tests, CancellationToken cancellationToken)
{
    await foreach (var update in notifications.WatchAsync(cancellationToken))
    {
        string? state = update.Snapshot.State?.Text;
        if (update.Resource != tests || update.Snapshot.ExitCode is not int exitCode || (state != KnownResourceStates.Finished && state != KnownResourceStates.Exited))
        {
            continue;
        }

        var result = exitCode == 0
            ? new ResourceStateSnapshot("Passed", KnownResourceStateStyles.Success)
            : new ResourceStateSnapshot("Failed", KnownResourceStateStyles.Error);

        await notifications.PublishUpdateAsync(tests, s => s with { State = result });
    }
}
