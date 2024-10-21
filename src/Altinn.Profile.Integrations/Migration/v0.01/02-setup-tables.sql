-- Create table MailboxSupplier
CREATE TABLE IF NOT EXISTS contact_and_reservation.mailbox_supplier (
    mailbox_supplier_id INT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    org_number_ak CHAR(9) NOT NULL,
    CONSTRAINT unique_org_number_ak UNIQUE (org_number_ak)
);

-- Create table Metadata
CREATE TABLE IF NOT EXISTS contact_and_reservation.metadata (
    latest_change_number BIGINT PRIMARY KEY,
    exported TIMESTAMPTZ
);

-- Create table Person
CREATE TABLE IF NOT EXISTS contact_and_reservation.person (
    contact_and_reservation_user_id INT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) PRIMARY KEY,
    fnumber_ak CHAR(11) NOT NULL UNIQUE,
    reservation BOOLEAN,
    description VARCHAR(20),
    mobile_phone_number VARCHAR(20),
    mobile_phone_number_last_updated TIMESTAMPTZ,
    mobile_phone_number_last_verified TIMESTAMPTZ,
    email_address VARCHAR(400),
    email_address_last_updated TIMESTAMPTZ,
    email_address_last_verified TIMESTAMPTZ,
    mailbox_address VARCHAR(50),
    mailbox_supplier_id_fk INT,
    x509_certificate TEXT,
    language_code CHAR(2) NULL,
    CONSTRAINT fk_mailbox_supplier FOREIGN KEY (mailbox_supplier_id_fk) REFERENCES contact_and_reservation.mailbox_supplier (mailbox_supplier_id),
    CONSTRAINT chk_language_code CHECK (language_code ~* '^[a-z]{2}$')
);

-- Indexes for performance
CREATE INDEX idx_fnumber_ak ON contact_and_reservation.person (fnumber_ak);
