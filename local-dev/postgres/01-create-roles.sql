-- Runs once, when the postgres volume is first created (docker-entrypoint-initdb.d).
-- The admin user and the profiledb database come from POSTGRES_USER / POSTGRES_DB.
-- The EF Core migrations grant schema and table permissions to platform_profile, so it must exist first.
CREATE ROLE platform_profile WITH LOGIN PASSWORD 'Password';
