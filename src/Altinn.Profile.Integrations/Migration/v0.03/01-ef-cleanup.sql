-- Unique index on fnumber_ak in person table instead of unique constraint
ALTER TABLE contact_and_reservation.person DROP CONSTRAINT IF EXISTS person_fnumber_ak_key;

CREATE UNIQUE INDEX IF NOT EXISTS person_fnumber_ak_key ON contact_and_reservation.person (fnumber_ak);

-- Drop unnecessary/duplicate index
DROP INDEX IF EXISTS contact_and_reservation.idx_fnumber_ak;

-- Drop check constraint chk_language_code in person table
ALTER TABLE contact_and_reservation.person DROP CONSTRAINT IF EXISTS chk_language_code

-- Unique index on org_number_ak in mailbox_supplier table instead of unique constraint
ALTER TABLE contact_and_reservation.mailbox_supplier DROP CONSTRAINT IF EXISTS unique_org_number_ak

CREATE UNIQUE INDEX IF NOT EXISTS unique_org_number_ak ON contact_and_reservation.mailbox_supplier (org_number_ak);

