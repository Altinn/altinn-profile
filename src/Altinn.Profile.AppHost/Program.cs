using System.Diagnostics.CodeAnalysis;

var builder = DistributedApplication.CreateBuilder(args);

string databaseName = "profiledb";
var profiledb = builder.AddPostgres("postgres", port: 32989)
    .WithEnvironment("POSTGRES_DB", databaseName)
    .WithDataVolume()
    .AddDatabase(databaseName);

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
