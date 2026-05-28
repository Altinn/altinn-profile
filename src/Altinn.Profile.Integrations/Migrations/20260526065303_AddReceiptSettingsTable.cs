using System;

using Altinn.Profile.Integrations.Persistence.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "receipt_settings",
                schema: "user_preferences",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    profile_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    user_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    request_receipt = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("userid_profiletype_pk", x => new { x.user_id, x.profile_type });
                });

            // Grant permissions to runtime user
            migrationBuilder.GrantTablePermissions("user_preferences", "receipt_settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optionally revoke permissions on rollback
            migrationBuilder.RevokeTablePermissions("user_preferences", "receipt_settings");
            migrationBuilder.DropTable(
                name: "receipt_settings",
                schema: "user_preferences");
        }
    }
}
