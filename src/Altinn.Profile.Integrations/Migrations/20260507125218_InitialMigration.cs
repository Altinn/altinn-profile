using System;
using System.Diagnostics.CodeAnalysis;

using Altinn.Profile.Integrations.Persistence.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Altinn.Profile.Integrations.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Comment out after initial migration has been applied, to prevent accidental data loss in case of re-application of the migration. */
            /*
            migrationBuilder.EnsureSchema(
                name: "lease");
            migrationBuilder.GrantSchemaPermissions("lease");

            migrationBuilder.EnsureSchema(
                name: "user_preferences");
            migrationBuilder.GrantSchemaPermissions("user_preferences");

            migrationBuilder.EnsureSchema(
                name: "contact_and_reservation");
            migrationBuilder.GrantSchemaPermissions("contact_and_reservation");

            migrationBuilder.EnsureSchema(
                name: "organization_notification_address");
            migrationBuilder.GrantSchemaPermissions("organization_notification_address");

            migrationBuilder.EnsureSchema(
                name: "professional_notification_settings");
            migrationBuilder.GrantSchemaPermissions("professional_notification_settings");

            migrationBuilder.EnsureSchema(
                name: "address_verifications");
            migrationBuilder.GrantSchemaPermissions("address_verifications");

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

            migrationBuilder.GrantTablePermissions("lease", "changelog_sync_metadata");

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "user_preferences",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("group_id_pkey", x => x.group_id);
                });

            migrationBuilder.GrantTablePermissions("user_preferences", "groups");

            migrationBuilder.CreateTable(
                name: "lease",
                schema: "lease",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    token = table.Column<Guid>(type: "uuid", nullable: false),
                    expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    acquired = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lease_id_pkey", x => x.id);
                });

            migrationBuilder.GrantTablePermissions("lease", "lease");

            migrationBuilder.CreateTable(
                name: "mailbox_supplier",
                schema: "contact_and_reservation",
                columns: table => new
                {
                    mailbox_supplier_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    org_number_ak = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mailbox_supplier_pkey", x => x.mailbox_supplier_id);
                });

            migrationBuilder.GrantTablePermissions("contact_and_reservation", "mailbox_supplier");

            migrationBuilder.CreateTable(
                name: "metadata",
                schema: "contact_and_reservation",
                columns: table => new
                {
                    latest_change_number = table.Column<long>(type: "bigint", nullable: false),
                    exported = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("metadata_pkey", x => x.latest_change_number);
                });

            migrationBuilder.GrantTablePermissions("contact_and_reservation", "metadata");

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "organization_notification_address",
                columns: table => new
                {
                    registry_organization_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    registry_organization_number = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_id_pkey", x => x.registry_organization_id);
                });

            migrationBuilder.GrantTablePermissions("organization_notification_address", "organizations");

            migrationBuilder.CreateTable(
                name: "profile_settings",
                schema: "user_preferences",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    language_type = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    do_not_prompt_for_party = table.Column<bool>(type: "boolean", nullable: false),
                    preselected_party_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    show_client_units = table.Column<bool>(type: "boolean", nullable: false),
                    should_show_sub_entities = table.Column<bool>(type: "boolean", nullable: false),
                    should_show_deleted_entities = table.Column<bool>(type: "boolean", nullable: false),
                    ignore_unit_profile_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_id_pkey", x => x.user_id);
                });

            migrationBuilder.GrantTablePermissions("user_preferences", "profile_settings");

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

            migrationBuilder.GrantTablePermissions("organization_notification_address", "registry_sync_metadata");

            migrationBuilder.CreateTable(
                name: "self_identified_users",
                schema: "user_preferences",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    user_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    email_address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    phone_number_last_changed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_self_identified_users", x => x.user_id);
                });

            migrationBuilder.GrantTablePermissions("user_preferences", "self_identified_users");

            migrationBuilder.CreateTable(
                name: "user_party_contact_info",
                schema: "professional_notification_settings",
                columns: table => new
                {
                    user_party_contact_info_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    party_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    email_address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: true),
                    last_changed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_party_contact_info_pkey", x => x.user_party_contact_info_id);
                });

            migrationBuilder.GrantTablePermissions("professional_notification_settings", "user_party_contact_info");

            migrationBuilder.CreateTable(
                name: "verification_codes",
                schema: "address_verifications",
                columns: table => new
                {
                    verification_code_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verification_code_hash = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    address_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("verification_code_id_pkey", x => x.verification_code_id);
                });

            migrationBuilder.GrantTablePermissions("address_verifications", "verification_codes");

            migrationBuilder.CreateTable(
                name: "verified_addresses",
                schema: "address_verifications",
                columns: table => new
                {
                    verified_address_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    address_type = table.Column<string>(type: "text", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_addresses", x => x.verified_address_id);
                });

            migrationBuilder.GrantTablePermissions("address_verifications", "verified_addresses");

            migrationBuilder.CreateTable(
                name: "party_group_association",
                schema: "user_preferences",
                columns: table => new
                {
                    association_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    party_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("association_id_pkey", x => x.association_id);
                    table.ForeignKey(
                        name: "fk_group_id",
                        column: x => x.group_id,
                        principalSchema: "user_preferences",
                        principalTable: "groups",
                        principalColumn: "group_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.GrantTablePermissions("user_preferences", "party_group_association");

            migrationBuilder.CreateTable(
                name: "person",
                schema: "contact_and_reservation",
                columns: table => new
                {
                    contact_and_reservation_user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fnumber_ak = table.Column<string>(type: "character(11)", fixedLength: true, maxLength: 11, nullable: false),
                    reservation = table.Column<bool>(type: "boolean", nullable: true),
                    description = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mobile_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mobile_phone_number_last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mobile_phone_number_last_verified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email_address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    email_address_last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email_address_last_verified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mailbox_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mailbox_supplier_id_fk = table.Column<int>(type: "integer", nullable: true),
                    x509_certificate = table.Column<string>(type: "text", nullable: true),
                    language_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("person_pkey", x => x.contact_and_reservation_user_id);
                    table.ForeignKey(
                        name: "fk_mailbox_supplier",
                        column: x => x.mailbox_supplier_id_fk,
                        principalSchema: "contact_and_reservation",
                        principalTable: "mailbox_supplier",
                        principalColumn: "mailbox_supplier_id");
                });

            migrationBuilder.GrantTablePermissions("contact_and_reservation", "person");

            migrationBuilder.CreateTable(
                name: "notifications_address",
                schema: "organization_notification_address",
                columns: table => new
                {
                    notification_address_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    registry_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    address_type = table.Column<int>(type: "integer", nullable: false),
                    domain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    full_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    registry_updated_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_source = table.Column<int>(type: "integer", nullable: false),
                    has_registry_accepted = table.Column<bool>(type: "boolean", nullable: true),
                    is_soft_deleted = table.Column<bool>(type: "boolean", nullable: true),
                    notification_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fk_registry_organization_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("contact_info_pkey", x => x.notification_address_id);
                    table.ForeignKey(
                        name: "fk_organization_id",
                        column: x => x.fk_registry_organization_id,
                        principalSchema: "organization_notification_address",
                        principalTable: "organizations",
                        principalColumn: "registry_organization_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.GrantTablePermissions("organization_notification_address", "notifications_address");

            migrationBuilder.CreateTable(
                name: "user_party_contact_info_resources",
                schema: "professional_notification_settings",
                columns: table => new
                {
                    user_party_contact_info_resource_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_party_contact_info_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_party_contact_info_resource_pkey", x => x.user_party_contact_info_resource_id);
                    table.ForeignKey(
                        name: "fk_user_party_contact_info_id",
                        column: x => x.user_party_contact_info_id,
                        principalSchema: "professional_notification_settings",
                        principalTable: "user_party_contact_info",
                        principalColumn: "user_party_contact_info_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.GrantTablePermissions("professional_notification_settings", "user_party_contact_info_resources");

            migrationBuilder.CreateIndex(
                name: "ix_groups_user_id",
                schema: "user_preferences",
                table: "groups",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_user_id_is_favorite",
                schema: "user_preferences",
                table: "groups",
                columns: new[] { "user_id", "is_favorite" },
                unique: true,
                filter: "is_favorite = true");

            migrationBuilder.CreateIndex(
                name: "ix_lease_id",
                schema: "lease",
                table: "lease",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mailbox_supplier_org_number_ak",
                schema: "contact_and_reservation",
                table: "mailbox_supplier",
                column: "org_number_ak",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_address_fk_registry_organization_id",
                schema: "organization_notification_address",
                table: "notifications_address",
                column: "fk_registry_organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_address_full_address",
                schema: "organization_notification_address",
                table: "notifications_address",
                column: "full_address");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_address_registry_id",
                schema: "organization_notification_address",
                table: "notifications_address",
                column: "registry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organizations_registry_organization_number",
                schema: "organization_notification_address",
                table: "organizations",
                column: "registry_organization_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_party_group_association_group_id",
                schema: "user_preferences",
                table: "party_group_association",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_fnumber_ak",
                schema: "contact_and_reservation",
                table: "person",
                column: "fnumber_ak",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_mailbox_supplier_id_fk",
                schema: "contact_and_reservation",
                table: "person",
                column: "mailbox_supplier_id_fk");

            migrationBuilder.CreateIndex(
                name: "ix_user_party_contact_info_email_address",
                schema: "professional_notification_settings",
                table: "user_party_contact_info",
                column: "email_address");

            migrationBuilder.CreateIndex(
                name: "ix_user_party_contact_info_party_uuid_user_id",
                schema: "professional_notification_settings",
                table: "user_party_contact_info",
                columns: new[] { "party_uuid", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_party_contact_info_phone_number",
                schema: "professional_notification_settings",
                table: "user_party_contact_info",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "ix_user_party_contact_info_user_id",
                schema: "professional_notification_settings",
                table: "user_party_contact_info",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_party_contact_info_resources_user_party_contact_info_id",
                schema: "professional_notification_settings",
                table: "user_party_contact_info_resources",
                column: "user_party_contact_info_id");

            migrationBuilder.CreateIndex(
                name: "ix_verification_codes_user_id_address_address_type",
                schema: "address_verifications",
                table: "verification_codes",
                columns: new[] { "user_id", "address", "address_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_addresses_user_id_address_address_type",
                schema: "address_verifications",
                table: "verified_addresses",
                columns: new[] { "user_id", "address", "address_type" },
                unique: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropTable(
                name: "changelog_sync_metadata",
                schema: "lease");

            migrationBuilder.DropTable(
                name: "lease",
                schema: "lease");

            migrationBuilder.DropTable(
                name: "metadata",
                schema: "contact_and_reservation");

            migrationBuilder.DropTable(
                name: "notifications_address",
                schema: "organization_notification_address");

            migrationBuilder.DropTable(
                name: "party_group_association",
                schema: "user_preferences");

            migrationBuilder.DropTable(
                name: "person",
                schema: "contact_and_reservation");

            migrationBuilder.DropTable(
                name: "profile_settings",
                schema: "user_preferences");

            migrationBuilder.DropTable(
                name: "registry_sync_metadata",
                schema: "organization_notification_address");

            migrationBuilder.DropTable(
                name: "self_identified_users",
                schema: "user_preferences");

            migrationBuilder.DropTable(
                name: "user_party_contact_info_resources",
                schema: "professional_notification_settings");

            migrationBuilder.DropTable(
                name: "verification_codes",
                schema: "address_verifications");

            migrationBuilder.DropTable(
                name: "verified_addresses",
                schema: "address_verifications");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "organization_notification_address");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "user_preferences");

            migrationBuilder.DropTable(
                name: "mailbox_supplier",
                schema: "contact_and_reservation");

            migrationBuilder.DropTable(
                name: "user_party_contact_info",
                schema: "professional_notification_settings");
            */
        }
    }
}
