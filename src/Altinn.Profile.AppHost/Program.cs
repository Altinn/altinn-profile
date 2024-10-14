using System.Diagnostics.CodeAnalysis;

var builder = DistributedApplication.CreateBuilder(args);

string databaseName = "profiledb";
var profiledb = builder.AddPostgres("postgres", port: 32989)
    .WithBindMount("../Altinn.Profile.Integrations/Migration", "/docker-entrypoint-initdb.d")
    .WithDataVolume()
    .AddAltinnDatabase("profile-db", databaseName: databaseName);

var registerApi = builder.AddProject<Projects.Altinn_Profile>("profile")
    .WithReference(profiledb);

builder.Build().Run();

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
