# Aspire app host

Starts the `profile-db` Postgres container, runs the database migrations, then starts Profile (port 5030), with logs, traces and metrics in the Aspire dashboard.

Requires the [Aspire CLI](https://aspire.dev/get-started/install-cli/) and Podman. `profile-db` is created from `profile-db/compose.yaml` if it does not exist.

```bash
cd tools/aspire
aspire run
```

Set `Postgres:UsePodman` to `false` in a git-ignored `appsettings.Local.json` to use a local PostgreSQL instead.
