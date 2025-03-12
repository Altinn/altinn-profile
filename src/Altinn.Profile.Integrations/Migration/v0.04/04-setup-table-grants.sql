-- Grant access to the tables
GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE organization_notification_address.notifications_address TO platform_profile;

GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE organization_notification_address.registry_sync_metadata TO platform_profile;

GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE organization_notification_address.organizations TO platform_profile;