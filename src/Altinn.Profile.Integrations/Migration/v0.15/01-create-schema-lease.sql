ALTER TABLE professional_notification_settings.user_party_contact_info ALTER COLUMN last_changed DROP DEFAULT;

ALTER TABLE lease.changelog_sync_metadata ADD nanosecond integer NOT NULL DEFAULT 0;