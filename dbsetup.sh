 #!/bin/bash
export PGPASSWORD=Password

# alter max connections
psql -h localhost -p 5432 -U platform_profile_admin -d profiledb \
-c "ALTER SYSTEM SET max_connections TO '200';"

# set up platform_profile role
psql -h localhost -p 5432 -U platform_profile_admin -d profiledb \
-c "DO \$\$
    BEGIN CREATE ROLE platform_profile WITH LOGIN  PASSWORD 'Password';
    EXCEPTION WHEN duplicate_object THEN RAISE NOTICE '%, skipping', SQLERRM USING ERRCODE = SQLSTATE;
    END \$\$;"
