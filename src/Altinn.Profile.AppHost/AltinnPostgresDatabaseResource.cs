using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Altinn.Profile.Aspire.AppHost;

/// <summary>
/// A resource that represents a PostgreSQL database. This is a child resource of
/// <see cref="PostgresServerResource"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class AltinnPostgresDatabaseResource
    : Resource
    , IResourceWithParent<PostgresServerResource>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnPostgresDatabaseResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="postgresParentResource">The PostgreSQL parent resource associated with this database.</param>
    /// <param name="roleNameParameter">A parameter that contains the PostgreSQL server role name used by the application, or <see langword="null"/> to use a default value.</param>
    /// <param name="passwordParameter">A parameter that contains the PostgreSQL server password used by the application.</param>
    /// <param name="ownerRoleNameParameter">A parameter that contains the PostgreSQL server role name for the database owner, or <see langword="null"/> to use a default value.</param>
    /// <param name="ownerPasswordParameter">A parameter that contains the PostgreSQL server password for the database owner.</param>
    /// <param name="migratorRoleNameParameter">A parameter that contains the PostgreSQL server role name for the database migrator, or <see langword="null"/> to use a default value.</param>
    /// <param name="migratorPasswordParameter">A parameter that contains the PostgreSQL server password for the database migrator.</param>
    /// <param name="seederRoleNameParameter">A parameter that contains the PostgreSQL server role name for the database seeder, or <see langword="null"/> to use a default value.</param>
    /// <param name="seederPasswordParameter">A parameter that contains the PostgreSQL server password for the database seeder.</param>
    public AltinnPostgresDatabaseResource(
        string name, 
        string databaseName, 
        PostgresServerResource postgresParentResource,
        ParameterResource? roleNameParameter,
        ParameterResource passwordParameter,
        ParameterResource? ownerRoleNameParameter,
        ParameterResource ownerPasswordParameter,
        ParameterResource? migratorRoleNameParameter,
        ParameterResource migratorPasswordParameter,
        ParameterResource? seederRoleNameParameter,
        ParameterResource seederPasswordParameter) 
        : base(name)
    {
        Parent = postgresParentResource;
        DatabaseName = databaseName;
        RoleNameParameter = roleNameParameter;
        PasswordParameter = passwordParameter;
        OwnerRoleNameParameter = ownerRoleNameParameter;
        OwnerPasswordParameter = ownerPasswordParameter;
        MigratorRoleNameParameter = migratorRoleNameParameter;
        MigratorPasswordParameter = migratorPasswordParameter;
        SeederRoleNameParameter = seederRoleNameParameter;
        SeederPasswordParameter = seederPasswordParameter;
    }

    /// <summary>
    /// Gets the parent PostgresSQL container resource.
    /// </summary>
    public PostgresServerResource Parent { get; }

    /// <summary>
    /// Gets the parameter that contains the PostgreSQL role name for the owner of the database.
    /// </summary>
    public ParameterResource? OwnerRoleNameParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the password for the owner of the database.
    /// </summary>
    public ParameterResource OwnerPasswordParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the PostgreSQL role name for the migrator of the database.
    /// </summary>
    public ParameterResource? MigratorRoleNameParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the password for the migrator of the database.
    /// </summary>
    public ParameterResource MigratorPasswordParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the PostgreSQL role name for the seeder of the database.
    /// </summary>
    public ParameterResource? SeederRoleNameParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the password for the seeder of the database.
    /// </summary>
    public ParameterResource SeederPasswordParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the PostgreSQL role name for the app-role of the database.
    /// </summary>
    public ParameterResource? RoleNameParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the password for the app-role of the database.
    /// </summary>
    public ParameterResource PasswordParameter { get; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Gets a <see cref="ReferenceExpression"/> for the role name of the application user of the database.
    /// </summary>
    internal ReferenceExpression RoleNameReference 
        => RoleNameParameter is not null 
        ? ReferenceExpression.Create($"{RoleNameParameter}") 
        : ReferenceExpression.Create($"{DatabaseName}");

    /// <summary>
    /// Gets a <see cref="ReferenceExpression"/> for the role name of the owner of the database.
    /// </summary>
    internal ReferenceExpression OwnerRoleNameReference
        => OwnerRoleNameParameter is not null
        ? ReferenceExpression.Create($"{OwnerRoleNameParameter}")
        : ReferenceExpression.Create($"{DatabaseName}-owner");

    /// <summary>
    /// Gets a <see cref="ReferenceExpression"/> for the role name of the migrator of the database.
    /// </summary>
    internal ReferenceExpression MigratorRoleNameReference
        => MigratorRoleNameParameter is not null
        ? ReferenceExpression.Create($"{MigratorRoleNameParameter}")
        : ReferenceExpression.Create($"{DatabaseName}-migrator");

    /// <summary>
    /// Gets a <see cref="ReferenceExpression"/> for the role name of the seeder of the database.
    /// </summary>
    internal ReferenceExpression SeederRoleNameReference
        => SeederRoleNameParameter is not null
        ? ReferenceExpression.Create($"{SeederRoleNameParameter}")
        : ReferenceExpression.Create($"{DatabaseName}-seeder");

    /// <summary>
    /// Gets the connection string expression for the application role.
    /// </summary>
    public ReferenceExpression ApplicationConnectionStringValueExpression
        => ReferenceExpression.Create($"Host={Parent.PrimaryEndpoint.Property(EndpointProperty.Host)};Port={Parent.PrimaryEndpoint.Property(EndpointProperty.Port)};Username={RoleNameReference};Password={PasswordParameter};Database={DatabaseName}");

    /// <summary>
    /// Gets the connection string expression for the database migrator role.
    /// </summary>
    public ReferenceExpression MigratorConnectionStringValueExpression
        => ReferenceExpression.Create($"Host={Parent.PrimaryEndpoint.Property(EndpointProperty.Host)};Port={Parent.PrimaryEndpoint.Property(EndpointProperty.Port)};Username={MigratorRoleNameReference};Password={MigratorPasswordParameter};Database={DatabaseName}");

    /// <summary>
    /// Gets the connection string expression for the database seeder role.
    /// </summary>
    public ReferenceExpression SeederConnectionStringValueExpression
        => ReferenceExpression.Create($"Host={Parent.PrimaryEndpoint.Property(EndpointProperty.Host)};Port={Parent.PrimaryEndpoint.Property(EndpointProperty.Port)};Username={SeederRoleNameReference};Password={SeederPasswordParameter};Database={DatabaseName}");

    /// <summary>
    /// Gets the connection string expression for the database creator role.
    /// </summary>
    public ReferenceExpression CreatorConnectionStringValueExpression
        => ReferenceExpression.Create($"Host={Parent.PrimaryEndpoint.Property(EndpointProperty.Host)};Port={Parent.PrimaryEndpoint.Property(EndpointProperty.Port)};Username={OwnerRoleNameReference};Password={OwnerPasswordParameter};Database={DatabaseName}");

    /// <summary>
    /// Gets the connection string expression for the database server.
    /// </summary>
    public ReferenceExpression ServerConnectionStringExpression
        => Parent.ConnectionStringExpression;

    /// <summary>
    /// Gets a property of the database resource.
    /// </summary>
    /// <param name="property">The property kind.</param>
    /// <returns>A <see cref="IValueProvider"/> for the property.</returns>
    public IValueProvider Property(DbProperty property)
        => property switch
        {
            // Connection strings
            DbProperty.ApplicationConnectionString => ConnectionString(ConnectionStrings.Application),
            DbProperty.MigratorConnectionString => ConnectionString(ConnectionStrings.Migrator),
            DbProperty.SeederConnectionString => ConnectionString(ConnectionStrings.Seeder),
            DbProperty.CreatorConnectionString => ConnectionString(ConnectionStrings.Creator),
            DbProperty.ServerConnectionString => new ConnectionStringReference(Parent, optional: false),

            // Role names
            DbProperty.ApplicationRoleName => RoleNameReference,
            DbProperty.OwnerRoleName => OwnerRoleNameReference,
            DbProperty.MigratorRoleName => MigratorRoleNameReference,
            DbProperty.SeederRoleName => SeederRoleNameReference,

            // Passwords
            DbProperty.ApplicationPassword => PasswordParameter,
            DbProperty.OwnerPassword => OwnerPasswordParameter,
            DbProperty.MigratorPassword => MigratorPasswordParameter,
            DbProperty.SeederPassword => SeederPasswordParameter,

            // Invalid
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<IValueProvider>(nameof(property)),
        };

    private ConnectionStringReference ConnectionString(ConnectionStrings connectionString)
        => new(new ConnectionStringResource(this, connectionString), optional: false);

    private enum ConnectionStrings
    {
        Application,
        Migrator,
        Seeder,
        Creator,
    }

    private sealed class ConnectionStringResource(AltinnPostgresDatabaseResource resource, ConnectionStrings connectionString)
        : IResourceWithConnectionString
    {
        ReferenceExpression IResourceWithConnectionString.ConnectionStringExpression 
            => connectionString switch
            {
                ConnectionStrings.Application => resource.ApplicationConnectionStringValueExpression,
                ConnectionStrings.Migrator => resource.MigratorConnectionStringValueExpression,
                ConnectionStrings.Seeder => resource.SeederConnectionStringValueExpression,
                ConnectionStrings.Creator => resource.CreatorConnectionStringValueExpression,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<ReferenceExpression>(nameof(connectionString)),
            };

        string IManifestExpressionProvider.ValueExpression
            => GetValueExpression();

        private string GetValueExpression()
        {
            var name = connectionString switch
            {
                ConnectionStrings.Application => "application",
                ConnectionStrings.Migrator => "migrator",
                ConnectionStrings.Seeder => "seeder",
                ConnectionStrings.Creator => "creator",
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(connectionString)),
            };

            return $"{{{Name}.connectionStrings.{name}}}";
        }

        public string Name 
            => resource.Name;

        ResourceAnnotationCollection IResource.Annotations 
            => resource.Annotations;
    }

    /// <summary>
    /// The different connection strings that can be used to connect to the database.
    /// </summary>
    public enum DbProperty
    {
        /// <summary>
        /// The connection string for the application role.
        /// </summary>
        ApplicationConnectionString,

        /// <summary>
        /// The connection string for the database migrator role.
        /// </summary>
        MigratorConnectionString,

        /// <summary>
        /// The connection string for the database seeder role.
        /// </summary>
        SeederConnectionString,

        /// <summary>
        /// The connection string for the database creator role.
        /// </summary>
        CreatorConnectionString,

        /// <summary>
        /// The connection string used to create the database.
        /// </summary>
        /// <remarks>
        /// This connection string is different from the others in that it points to the database server,
        /// not the current database. It's intended use is to create the database itself.
        /// </remarks>
        ServerConnectionString,

        /// <summary>
        /// The name of the application role.
        /// </summary>
        ApplicationRoleName,

        /// <summary>
        /// The password of the application role.
        /// </summary>
        ApplicationPassword,

        /// <summary>
        /// The name of the owner role.
        /// </summary>
        OwnerRoleName,

        /// <summary>
        /// The password of the owner role.
        /// </summary>
        OwnerPassword,

        /// <summary>
        /// The name of the migrator role.
        /// </summary>
        MigratorRoleName,

        /// <summary>
        /// The password of the migrator role.
        /// </summary>
        MigratorPassword,

        /// <summary>
        /// The name of the seeder role.
        /// </summary>
        SeederRoleName,

        /// <summary>
        /// The password of the seeder role.
        /// </summary>
        SeederPassword,
    }
}
