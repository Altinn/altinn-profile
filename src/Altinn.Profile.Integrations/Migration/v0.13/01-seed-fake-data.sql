DO $$
	DECLARE lastid int;
BEGIN
	INSERT INTO organization_notification_address.organizations (registry_organization_number)
	values ('810889802')
	Returning registry_organization_id INTO lastid;

	INSERT INTO organization_notification_address.notifications_address(registry_id, address_type, domain, address, full_address, created_date_time, registry_updated_date_time, update_source, has_registry_accepted, is_soft_deleted, notification_name, fk_registry_organization_id)
	values (1, 2, 'default.digdir.no', 'nullstillt', 'nullstillt@default.digdir.no', now(), now(), 3, true, false, NULL, lastid);

	INSERT INTO organization_notification_address.notifications_address(registry_id, address_type, domain, address, full_address, created_date_time, registry_updated_date_time, update_source, has_registry_accepted, is_soft_deleted, notification_name, fk_registry_organization_id)
	values (2, 1, '+47', '99999999', '+4799999999', now(), now(), 3, true, false, NULL, lastid);
END $$;