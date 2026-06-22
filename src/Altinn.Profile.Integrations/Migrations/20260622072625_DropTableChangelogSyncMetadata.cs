using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class DropTableChangelogSyncMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "changelog_sync_metadata",
                schema: "lease");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "changelog_sync_metadata",
                schema: "lease",
                columns: table => new
                {
                    last_changed_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    data_type = table.Column<int>(type: "integer", nullable: false),
                    last_change_ticks = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("changelog_sync_metadata_pkey", x => x.last_changed_id);
                });
        }
    }
}
