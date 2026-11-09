using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class ExtendOrgAddressLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "full_address",
                schema: "organization_notification_address",
                table: "notifications_address",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "domain",
                schema: "organization_notification_address",
                table: "notifications_address",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                schema: "organization_notification_address",
                table: "notifications_address",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "full_address",
                schema: "organization_notification_address",
                table: "notifications_address",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);

            migrationBuilder.AlterColumn<string>(
                name: "domain",
                schema: "organization_notification_address",
                table: "notifications_address",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                schema: "organization_notification_address",
                table: "notifications_address",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);
        }
    }
}
