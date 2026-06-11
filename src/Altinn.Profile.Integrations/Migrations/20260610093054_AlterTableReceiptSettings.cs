using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableReceiptSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "userid_profiletype_pk",
                schema: "user_preferences",
                table: "receipt_settings");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "user_preferences",
                table: "receipt_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddPrimaryKey(
                name: "id_pkey",
                schema: "user_preferences",
                table: "receipt_settings",
                column: "id");
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

            migrationBuilder.AddPrimaryKey(
                name: "userid_profiletype_pk",
                schema: "user_preferences",
                table: "receipt_settings",
                columns: new[] { "user_id", "profile_type" });
        }
    }
}
