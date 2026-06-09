using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class AltertableReceiptSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "user_preferences",
                table: "receipt_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<bool>(
                name: "is_private",
                schema: "user_preferences",
                table: "receipt_settings",
                type: "boolean",
                nullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "userid_profiletype_pk",
                schema: "user_preferences",
                table: "receipt_settings");

            migrationBuilder.AddPrimaryKey(
                name: "id_pkey",
                schema: "user_preferences",
                table: "receipt_settings",
                column: "id");

            migrationBuilder.Sql("""
                UPDATE user_preferences.receipt_settings
                SET is_private = CASE profile_type
                    WHEN 'Private' THEN TRUE
                    ELSE FALSE
                END;
                """);

            migrationBuilder.AlterColumn<bool>(
                name: "is_private",
                schema: "user_preferences",
                table: "receipt_settings",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "profile_type",
                schema: "user_preferences",
                table: "receipt_settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "id_pkey",
                schema: "user_preferences",
                table: "receipt_settings");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "user_preferences",
                table: "receipt_settings");

            migrationBuilder.DropColumn(
                name: "is_private",
                schema: "user_preferences",
                table: "receipt_settings");

            migrationBuilder.AddColumn<string>(
                name: "profile_type",
                schema: "user_preferences",
                table: "receipt_settings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "userid_profiletype_pk",
                schema: "user_preferences",
                table: "receipt_settings",
                columns: new[] { "user_id", "profile_type" });
        }
    }
}
