-- Create table Organizations
CREATE TABLE IF NOT EXISTS organization_notification_address.organizations (
    registry_organization_id VARCHAR(32) NOT NULL,
    registry_organization_id INT NOT NULL,
    CONSTRAINT organization_id_pkey PRIMARY KEY (registry_organization_id)
);


-- Create table RegistrySyncMetadata
CREATE TABLE IF NOT EXISTS organization_notification_address.registry_sync_metadata (
    last_changed_id VARCHAR(32) NOT NULL,
    last_changed_date_time TIMESTAMPTZ NOT NULL,
    CONSTRAINT registry_sync_metadata_pkey PRIMARY KEY (last_changed_id)
);