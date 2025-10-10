CREATE TABLE user_preferences.profile_settings (
    user_id integer NOT NULL,
    language_type character varying(2) NOT NULL,
    do_not_prompt_for_party boolean NOT NULL,
    preselected_party_uuid uuid,
    show_client_units boolean NOT NULL,
    should_show_sub_entities boolean NOT NULL,
    should_show_deleted_entities boolean NOT NULL,
    ignore_unit_profile_date_time timestamp with time zone,
    CONSTRAINT user_id_pkey PRIMARY KEY (user_id)
);