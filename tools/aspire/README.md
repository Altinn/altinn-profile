# Aspire app host

Starts the `profile-db` Postgres container, runs the database migrations, then starts Profile (port 5030), with logs, traces and metrics in the Aspire dashboard.

Requires the [Aspire CLI](https://aspire.dev/get-started/install-cli/) and Podman or Docker. `profile-db` is created from `profile-db/compose.yaml` if it does not exist.

```bash
cd tools/aspire
aspire run
```

Without a trusted .NET development certificate, run `aspire run -lp http` to get the dashboard on `http://localhost:15193` with no login token. Use it on your own machine only.

## Settings

Override `appsettings.json` in a git-ignored `appsettings.Local.json`.

| Setting | Default | Description |
| :--- | :--- | :--- |
| `ContainerRuntime` | `podman` | `podman` or `docker` |
| `Postgres:UseContainer` | `true` | `false` uses a local PostgreSQL instead of the container |
| `Postgres:ContainerName` | `profile-db` | Another name creates a separate container and volume |
| `Postgres:Port` | `5432` | Host port for the database, e.g. to run next to an existing one |
