CREATE SCHEMA IF NOT EXISTS address_verifications;

GRANT ALL ON SCHEMA address_verifications TO platform_profile_admin;
GRANT USAGE ON SCHEMA address_verifications TO platform_profile;

CREATE TABLE address_verifications.verification_codes (
    verification_code_id integer GENERATED ALWAYS AS IDENTITY,
    user_id integer NOT NULL,
    failed_attempts integer NOT NULL,
    expires timestamp with time zone NOT NULL,
    created timestamp with time zone NOT NULL,
    verification_code_hash text NOT NULL,
    address text NOT NULL,
    address_type text NOT NULL,
    CONSTRAINT verification_code_id_pkey PRIMARY KEY (verification_code_id)
);

CREATE TABLE address_verifications.verified_addresses (
    verified_address_id integer GENERATED ALWAYS AS IDENTITY,
    user_id integer NOT NULL,
    address text NOT NULL,
    address_type text NOT NULL,
    verified_at timestamp with time zone NOT NULL DEFAULT (now()),
    verification_type integer NOT NULL,
    CONSTRAINT pk_verified_addresses PRIMARY KEY (verified_address_id)
);

CREATE INDEX ix_verification_codes_user_id_address_address_type ON address_verifications.verification_codes (user_id, address, address_type);

CREATE INDEX ix_verified_addresses_user_id_address_address_type ON address_verifications.verified_addresses (user_id, address, address_type);



GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE address_verifications.verification_codes TO platform_profile;

GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE address_verifications.verified_addresses TO platform_profile;