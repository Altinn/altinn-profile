CREATE SCHEMA IF NOT EXISTS user_preferences;

-- Grant access to the schema
GRANT ALL ON SCHEMA user_preferences TO platform_profile_admin;
GRANT USAGE ON SCHEMA user_preferences TO platform_profile;

CREATE TABLE user_preferences.groups (
    group_id integer GENERATED BY DEFAULT AS IDENTITY,
    name text NOT NULL,
    user_id integer NOT NULL,
    is_favorite boolean NOT NULL,
    CONSTRAINT group_id_pkey PRIMARY KEY (group_id)
);

CREATE TABLE user_preferences.party_group_association (
    association_id integer GENERATED BY DEFAULT AS IDENTITY,
    group_id integer NOT NULL,
    party_id integer NOT NULL,
    created timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT association_id_pkey PRIMARY KEY (association_id),
    CONSTRAINT fk_group_id FOREIGN KEY (group_id) REFERENCES user_preferences.groups (group_id) ON DELETE CASCADE
);

CREATE INDEX ix_groups_user_id ON user_preferences.groups (user_id);

CREATE INDEX ix_party_group_association_group_id ON user_preferences.party_group_association (group_id);

-- Grant access to the tables
GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE user_preferences.party_group_association TO platform_profile;

GRANT DELETE, INSERT, SELECT, UPDATE  ON TABLE user_preferences.groups TO platform_profile;
