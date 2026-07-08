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
            migrationBuilder.Sql(
                "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_self_identified_users_phone_number ON user_preferences.self_identified_users (phone_number);");

            migrationBuilder.Sql(
                "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_person_mobile_phone_number ON contact_and_reservation.person (mobile_phone_number);");

            migrationBuilder.Sql(
                "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_person_email_address_lower ON  contact_and_reservation.person (lower(email_address));");
            migrationBuilder.Sql(
                "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_self_identified_users_email_address_lower ON user_preferences.self_identified_users (lower(email_address));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_self_identified_users_phone_number",
                schema: "user_preferences",
                table: "self_identified_users");

            migrationBuilder.DropIndex(
                name: "ix_person_mobile_phone_number",
                schema: "contact_and_reservation",
                table: "person");

            migrationBuilder.DropIndex(
                name: "ix_person_email_address_lower",
                schema: "contact_and_reservation",
                table: "person");

            migrationBuilder.DropIndex(
                name: "ix_self_identified_users_email_address_lower",
                schema: "user_preferences",
                table: "self_identified_users");
        }
    }
}
