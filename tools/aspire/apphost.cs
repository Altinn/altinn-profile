#:sdk Aspire.AppHost.Sdk@13.6.1
#:property AspireUseCliBundle=true

using System.Diagnostics;
using Microsoft.Extensions.Configuration;

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
builder.AddProject("profile", ProfileProject, launchProfileName: null)
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

builder.Build().Run();

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
