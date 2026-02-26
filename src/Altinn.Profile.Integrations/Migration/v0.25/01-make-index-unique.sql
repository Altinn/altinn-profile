DROP INDEX IF EXISTS address_verifications.ix_verification_codes_user_id_address_address_type;

CREATE UNIQUE INDEX IF NOT EXISTS ix_verification_codes_user_id_address_address_type ON address_verifications.verification_codes (user_id, address, address_type);

