using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class AddTableUnitProfileStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "unit_profile_status",
                schema: "organization_notification_address",
                columns: table => new
                {
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    party_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    last_modified_by_user_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_confirmed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    last_confirmed_by_user_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    last_confirmation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit_profiles", x => x.party_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unit_profile_status",
                schema: "organization_notification_address");
        }
    }
}
