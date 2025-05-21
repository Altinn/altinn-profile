ALTER TABLE user_preferences.party_group_association DROP COLUMN party_id;

ALTER TABLE user_preferences.party_group_association ADD party_uuid uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
