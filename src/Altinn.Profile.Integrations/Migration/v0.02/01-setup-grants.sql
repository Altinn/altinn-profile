-- Grant access to the mailbox_supplier table
GRANT SELECT,INSERT,UPDATE,DELETE ON TABLE contact_and_reservation.mailbox_supplier TO platform_profile;

-- Grant access to the metadata table
GRANT SELECT,INSERT,UPDATE,DELETE ON TABLE contact_and_reservation.metadata TO platform_profile;

-- Grant access to the person table
GRANT SELECT,INSERT,UPDATE,DELETE ON TABLE contact_and_reservation.person TO platform_profile;
