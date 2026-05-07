using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Altinn.Profile.Integrations.Persistence.Migrations;

/// <summary>
/// Extension methods for MigrationBuilder to help with database grants.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MigrationBuilderExtensions
{
    /// <summary>
    /// Adds a grant statement for the runtime database user to the migration.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="schema">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="permissions">The permissions to grant (default: SELECT, INSERT, UPDATE, DELETE).</param>
    /// <example>
    /// <code>
    /// protected override void Up(MigrationBuilder migrationBuilder)
    /// {
    ///     migrationBuilder.CreateTable(
    ///         name: "my_table",
    ///         schema: "public",
    ///         columns: table => new { /* ... */ });
    ///     
    ///     migrationBuilder.GrantTablePermissions("public", "my_table");
    /// }
    /// </code>
    /// </example>
    public static void GrantTablePermissions(
        this MigrationBuilder migrationBuilder,
        string schema,
        string tableName,
        string permissions = "SELECT, INSERT, UPDATE, DELETE")
    {
        var grantSql = $"GRANT {permissions} ON TABLE {schema}.{tableName} TO platform_profile;";
        migrationBuilder.Sql(grantSql);
    }

    /// <summary>
    /// Revokes permissions from the runtime database user for a table.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="schema">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="permissions">The permissions to revoke (default: SELECT, INSERT, UPDATE, DELETE).</param>
    public static void RevokeTablePermissions(
        this MigrationBuilder migrationBuilder,
        string schema,
        string tableName,
        string permissions = "SELECT, INSERT, UPDATE, DELETE")
    {
        var revokeSql = $"REVOKE {permissions} ON TABLE {schema}.{tableName} FROM platform_profile;";
        migrationBuilder.Sql(revokeSql, suppressTransaction: true);
    }
}
