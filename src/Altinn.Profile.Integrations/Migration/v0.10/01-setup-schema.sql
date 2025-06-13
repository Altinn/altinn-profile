CREATE SCHEMA IF NOT EXISTS professional_notifications;

GRANT ALL ON SCHEMA professional_notifications TO platform_profile_admin;
GRANT USAGE ON SCHEMA professional_notifications TO platform_profile;

REATE TABLE professional_notifications.user_party_contact_info (
    user_party_contact_info_id bigint GENERATED ALWAYS AS IDENTITY,
    user_id integer NOT NULL,
    party_uuid uuid NOT NULL,
    email_address character varying(400),
    phone_number character varying(26),
    last_changed timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT user_party_contact_info_pkey PRIMARY KEY (user_party_contact_info_id)
);

CREATE TABLE professional_notifications.user_party_contact_info_resources (
    user_party_contact_info_resource_id bigint GENERATED ALWAYS AS IDENTITY,
    user_party_contact_info_id bigint NOT NULL,
    resource_id text NOT NULL,
    CONSTRAINT user_party_contact_info_resource_pkey PRIMARY KEY (user_party_contact_info_resource_id),
    CONSTRAINT fk_user_party_contact_info_id FOREIGN KEY (user_party_contact_info_id) REFERENCES professional_notifications.user_party_contact_info (user_party_contact_info_id) ON DELETE CASCADE
);

CREATE INDEX ix_user_party_contact_info_party_uuid_user_id ON professional_notifications.user_party_contact_info (party_uuid, user_id);

CREATE INDEX ix_user_party_contact_info_resources_user_party_contact_info_id ON professional_notifications.user_party_contact_info_resources (user_party_contact_info_id);