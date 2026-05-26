# Database Grants for EF Core Migrations

When creating new tables in Entity Framework Core migrations, you need to grant permissions to the runtime database user (`platform_profile`) so the application can access the tables.

## Usage

Use the `GrantTablePermissions` extension method in your migration's `Up` method:

```csharp
public partial class CreateMyNewTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "my_table",
            schema: "my_schema",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                // ... other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_my_table", x => x.id);
            });

        // Grant permissions to runtime user
        migrationBuilder.GrantTablePermissions("my_schema", "my_table");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Optionally revoke permissions on rollback
        migrationBuilder.RevokeTablePermissions("my_schema", "my_table");

        migrationBuilder.DropTable(
            name: "my_table",
            schema: "my_schema");
    }
}
```

## Available Methods

### GrantTablePermissions

Adds a GRANT statement to give `platform_profile` user access to a table.

```csharp
migrationBuilder.GrantTablePermissions(schema: "public", tableName: "my_table");
```

**Default permissions:** `SELECT, INSERT, UPDATE, DELETE`

**Custom permissions:**
```csharp
migrationBuilder.GrantTablePermissions("public", "my_table", "SELECT, INSERT");
```

### RevokeTablePermissions

Removes permissions from `platform_profile` user on a table (used in `Down` methods).

```csharp
migrationBuilder.RevokeTablePermissions("public", "my_table");
```

## Notes

- These helpers are defined in `Altinn.Profile.Integrations.Persistence.Migrations.MigrationBuilderExtensions`
- They use the admin connection string automatically (migrations run with admin privileges)
- The `suppressTransaction: true` flag on `RevokeTablePermissions` ensures it doesn't fail if the transaction doesn't support DDL
