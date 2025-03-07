using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaOrgNotificationAdress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "organization_notification_address");

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "organization_notification_address",
                columns: table => new
                {
                    registry_organization_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    registry_organization_number = table.Column<int>(type: "integer", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_id_pkey", x => x.registry_organization_id);
                });

            migrationBuilder.CreateTable(
                name: "registry_sync_metadata",
                schema: "organization_notification_address",
                columns: table => new
                {
                    last_changed_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    last_changed_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("registry_sync_metadata_pkey", x => x.last_changed_id);
                });

            migrationBuilder.CreateTable(
                name: "notifications_address",
                schema: "organization_notification_address",
                columns: table => new
                {
                    contact_info_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    registry_organization_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    registry_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    address_type = table.Column<int>(type: "integer", nullable: false),
                    domain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    full_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    registry_updated_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_source = table.Column<int>(type: "integer", nullable: false),
                    has_registry_accepted = table.Column<bool>(type: "boolean", nullable: true),
                    is_soft_deleted = table.Column<bool>(type: "boolean", nullable: true),
                    notification_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrganizationRegistryOrganizationId = table.Column<string>(type: "character varying(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("contact_info_pkey", x => x.contact_info_id);
                    table.UniqueConstraint("registry_organization_id_akey", x => x.registry_organization_id);
                    table.ForeignKey(
                        name: "FK_notifications_address_organizations_OrganizationRegistryOrg~",
                        column: x => x.OrganizationRegistryOrganizationId,
                        principalSchema: "organization_notification_address",
                        principalTable: "organizations",
                        principalColumn: "registry_organization_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_address_OrganizationRegistryOrganizationId",
                schema: "organization_notification_address",
                table: "notifications_address",
                column: "OrganizationRegistryOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_registry_organization_number",
                schema: "organization_notification_address",
                table: "organizations",
                column: "registry_organization_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications_address",
                schema: "organization_notification_address");

            migrationBuilder.DropTable(
                name: "registry_sync_metadata",
                schema: "organization_notification_address");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "organization_notification_address");
        }
    }
}
