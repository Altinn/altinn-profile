CREATE SCHEMA IF NOT EXISTS lease;

GRANT ALL ON SCHEMA lease TO platform_profile_admin;
GRANT USAGE ON SCHEMA lease TO platform_profile;

CREATE TABLE lease.changelog_sync_metadata (
    last_changed_id character varying(32) NOT NULL,
    last_changed_date_time timestamp with time zone NOT NULL,
    data_type integer NOT NULL,
    CONSTRAINT changelog_sync_metadata_pkey PRIMARY KEY (last_changed_id)
);

CREATE TABLE lease.lease (
    id text NOT NULL,
    token uuid NOT NULL,
    expires timestamp with time zone NOT NULL,
    acquired timestamp with time zone,
    released timestamp with time zone,
    CONSTRAINT lease_id_pkey PRIMARY KEY (id)
);

CREATE UNIQUE INDEX ix_lease_id ON lease.lease (id);
