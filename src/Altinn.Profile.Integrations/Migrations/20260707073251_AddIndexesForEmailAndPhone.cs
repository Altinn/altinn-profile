using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForEmailAndPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_self_identified_users_email_address",
                schema: "user_preferences",
                table: "self_identified_users",
                column: "email_address");

            migrationBuilder.CreateIndex(
                name: "ix_self_identified_users_phone_number",
                schema: "user_preferences",
                table: "self_identified_users",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "ix_person_email_address",
                schema: "contact_and_reservation",
                table: "person",
                column: "email_address");

            migrationBuilder.CreateIndex(
                name: "ix_person_mobile_phone_number",
                schema: "contact_and_reservation",
                table: "person",
                column: "mobile_phone_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_self_identified_users_email_address",
                schema: "user_preferences",
                table: "self_identified_users");

            migrationBuilder.DropIndex(
                name: "ix_self_identified_users_phone_number",
                schema: "user_preferences",
                table: "self_identified_users");

            migrationBuilder.DropIndex(
                name: "ix_person_email_address",
                schema: "contact_and_reservation",
                table: "person");

            migrationBuilder.DropIndex(
                name: "ix_person_mobile_phone_number",
                schema: "contact_and_reservation",
                table: "person");
        }
    }
}
