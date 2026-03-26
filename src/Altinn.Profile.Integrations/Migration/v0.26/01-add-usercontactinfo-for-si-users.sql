CREATE TABLE user_preferences.self_identified_users (
    user_id integer NOT NULL,
    user_uuid uuid NOT NULL,
    username text NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (now()),
    email_address character varying(400) NOT NULL,
    phone_number character varying(26),
    phone_number_last_changed timestamp with time zone,
    CONSTRAINT pk_self_identified_users PRIMARY KEY (user_id)
);