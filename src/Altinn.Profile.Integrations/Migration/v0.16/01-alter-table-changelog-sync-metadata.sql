ALTER TABLE lease.changelog_sync_metadata DROP COLUMN IF EXISTS last_changed_date_time;

ALTER TABLE lease.changelog_sync_metadata DROP COLUMN IF EXISTS nanosecond;

ALTER TABLE lease.changelog_sync_metadata ADD IF NOT EXISTS last_change_ticks bigint NOT NULL DEFAULT 0;