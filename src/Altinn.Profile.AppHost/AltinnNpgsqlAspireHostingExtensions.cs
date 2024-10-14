using System.Diagnostics.CodeAnalysis;
using Altinn.Profile.Aspire.AppHost;

using static Altinn.Profile.Aspire.AppHost.AltinnPostgresDatabaseResource;

namespace Aspire.Hosting;

/// <summary>
/// Extension methods for defining postgresql databases in an Aspire application preconfigured.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnAspireHostingExtensions
{
    /// <summary>
    /// Adds a PostgreSQL database to the application model.
    /// </summary>
    /// <param name="builder">The PostgreSQL server resource builder.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="databaseName">The name of the database. If not provided, this defaults to the same value as <paramref name="name"/>.</param>
    /// <param name="roleName">The parameter used to provide the role name for the PostgreSQL resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the password for the PostgreSQL resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="ownerRoleName">The parameter used to provide the role name for the db-owner role. If <see langword="null"/> a default value will be used.</param>
    /// <param name="ownerPassword">The parameter used to provide the password for db-owner role of the PostgreSQL resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="migratorRoleName">The parameter used to provide the role name for the db-migrator role. If <see langword="null"/> a default value will be used.</param>
    /// <param name="migratorPassword">The parameter used to provide the password for db-migrator role of the PostgreSQL resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="seederRoleName">The parameter used to provide the role name for the db-seeder role. If <see langword="null"/> a default value will be used.</param>
    /// <param name="seederPassword">The parameter used to provide the password for db-seeder role of the PostgreSQL resource. If <see langword="null"/> a random password will be generated.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<AltinnPostgresDatabaseResource> AddAltinnDatabase(
        this IResourceBuilder<PostgresServerResource> builder,
        string name,
        string? databaseName = null,
        IResourceBuilder<ParameterResource>? roleName = null,
        IResourceBuilder<ParameterResource>? password = null,
        IResourceBuilder<ParameterResource>? ownerRoleName = null,
        IResourceBuilder<ParameterResource>? ownerPassword = null,
        IResourceBuilder<ParameterResource>? migratorRoleName = null,
        IResourceBuilder<ParameterResource>? migratorPassword = null,
        IResourceBuilder<ParameterResource>? seederRoleName = null,
        IResourceBuilder<ParameterResource>? seederPassword = null)
    {
        // Use the resource name as the database name if it's not provided
        databaseName ??= name;

        var passwordParameter = password?.Resource 
            ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder.ApplicationBuilder, $"{databaseName}-password");
        var ownerPasswordParameter = ownerPassword?.Resource 
            ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder.ApplicationBuilder, $"{databaseName}-owner-password");
        var migratorPasswordParameter = migratorPassword?.Resource 
            ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder.ApplicationBuilder, $"{databaseName}-migrator-password");
        var seederPasswordParameter = seederPassword?.Resource
            ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder.ApplicationBuilder, $"{databaseName}-seeder-password");

        var altinnDatabase = new AltinnPostgresDatabaseResource(
            name,
            databaseName,
            builder.Resource,
            roleName?.Resource,
            passwordParameter,
            ownerRoleName?.Resource,
            ownerPasswordParameter,
            migratorRoleName?.Resource,
            migratorPasswordParameter,
            seederRoleName?.Resource,
            seederPasswordParameter);

        return builder.ApplicationBuilder.AddResource(altinnDatabase);
    }

    /// <summary>
    /// Injects Altinn specific environment variables from the source resource into the destination resource,
    /// using the source resource's name as the connection string name (if not overridden).
    /// </summary>
    /// <typeparam name="TDestination">The destination resource.</typeparam>
    /// <param name="builder">The resource where connection string will be injected.</param>
    /// <param name="source">The resource from which to extract the database configuration.</param>
    /// <param name="connectionName">An override of the source resource's name for the connection string.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(
        this IResourceBuilder<TDestination> builder, 
        IResourceBuilder<AltinnPostgresDatabaseResource> source, 
        string? connectionName = null)
        where TDestination : IResourceWithEnvironment
    {
        var resource = source.Resource;
        connectionName ??= resource.Name;

        var callback = CreateDatabaseResourceEnvironmentPopulationCallback(resource, connectionName);
        return builder.WithEnvironment(callback);
    }

    private static Action<EnvironmentCallbackContext> CreateDatabaseResourceEnvironmentPopulationCallback(AltinnPostgresDatabaseResource resource, string connectionName)
    {
        return (context) =>
        {
            var env = context.EnvironmentVariables;
            var prefix = $"Altinn__Npgsql__{connectionName}__";

            env[$"{prefix}ConnectionString"] = resource.Property(DbProperty.ApplicationConnectionString);
            env[$"{prefix}Migrate__ConnectionString"] = resource.Property(DbProperty.MigratorConnectionString);
            env[$"{prefix}Seed__ConnectionString"] = resource.Property(DbProperty.SeederConnectionString);
            env[$"{prefix}Create__DatabaseConnectionString"] = resource.Property(DbProperty.CreatorConnectionString);
            env[$"{prefix}Create__ClusterConnectionString"] = resource.Property(DbProperty.ServerConnectionString);

            env[$"{prefix}Migrate__Enabled"] = "true";
            env[$"{prefix}Seed__Enabled"] = "true";
            env[$"{prefix}Create__Enabled"] = "true";

            env[$"{prefix}Create__DatabaseName"] = resource.DatabaseName;
            env[$"{prefix}Create__DatabaseOwner"] = "owner";

            env[$"{prefix}Yuniql__MigrationsTable__Schema"] = "yuniql";
            env[$"{prefix}Create__Schemas__yuniql__Name"] = "yuniql";
            env[$"{prefix}Create__Schemas__yuniql__Owner"] = "migrator";

            env[$"{prefix}Create__Roles__owner__Name"] = resource.Property(DbProperty.OwnerRoleName);
            env[$"{prefix}Create__Roles__owner__Password"] = resource.Property(DbProperty.OwnerPassword);
            env[$"{prefix}Create__Roles__migrator__Name"] = resource.Property(DbProperty.MigratorRoleName);
            env[$"{prefix}Create__Roles__migrator__Password"] = resource.Property(DbProperty.MigratorPassword);
            env[$"{prefix}Create__Roles__seeder__Name"] = resource.Property(DbProperty.SeederRoleName);
            env[$"{prefix}Create__Roles__seeder__Password"] = resource.Property(DbProperty.SeederPassword);
            env[$"{prefix}Create__Roles__app__Name"] = resource.Property(DbProperty.ApplicationRoleName);
            env[$"{prefix}Create__Roles__app__Password"] = resource.Property(DbProperty.ApplicationPassword);

            env[$"{prefix}Create__Roles__owner__Grants__Roles__migrator__Usage"] = "true";
            env[$"{prefix}Create__Roles__owner__Grants__Roles__seeder__Usage"] = "true";
            env[$"{prefix}Create__Roles__owner__Grants__Roles__app__Usage"] = "true";

            env[$"{prefix}Create__Roles__migrator__Grants__Database__Privileges"] = "Connect,Create";

            env[$"{prefix}Create__Roles__seeder__Grants__Database__Privileges"] = "Connect";
            env[$"{prefix}Create__Roles__seeder__Grants__Roles__app__Usage"] = "true";
            env[$"{prefix}Create__Roles__seeder__Grants__Roles__migrator__Usage"] = "true";

            env[$"{prefix}Create__Roles__app__Grants__Database__Privileges"] = "Connect";
        };
    }
}
