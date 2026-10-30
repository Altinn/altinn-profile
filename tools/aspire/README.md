# Aspire app host

Starts the `profile-db` Postgres container, runs the database migrations, then starts Profile (port 5030), with logs, traces and metrics in the Aspire dashboard.

Requires the [Aspire CLI](https://aspire.dev/get-started/install-cli/) and Docker or Podman. The app host uses whichever is running. `profile-db` is created from `profile-db/compose.yaml` if it does not exist.

```bash
cd tools/aspire
aspire run
```

The dashboard is on `https://localhost:17230`. Without a trusted .NET development certificate, run `aspire run -lp http --non-interactive` to get the dashboard on `http://localhost:15230` with no login token (`--non-interactive` stops the CLI from asking to trust the certificate). Use it on your own machine only.

## Running next to other app hosts

The ports are chosen so this app host can run at the same time as the one in altinn-notifications:

| What | Port |
| :--- | :--- |
| Profile | `5030`, the same as `dotnet run` |
| Postgres (`profile-db`) | `5442` |
| Dashboard | `17230` (https), `15230` (http) |
| OTLP and resource service | `21230` and `22230` (https), `19230` and `20230` (http) |

## Settings

Override `appsettings.json` in a git-ignored `appsettings.Local.json`.

| Setting | Default | Description |
| :--- | :--- | :--- |
| `ContainerRuntime` | (none) | `docker` or `podman`. Without it, `ASPIRE_CONTAINER_RUNTIME` is used if set, or else the first of `docker` and `podman` that is running |
| `Postgres:UseContainer` | `true` | `false` uses a local PostgreSQL instead of the container. Set `Postgres:Port` to its port, usually `5432` |
| `Postgres:ContainerName` | `profile-db` | Another name creates a separate container and volume |
| `Postgres:Port` | `5442` | Host port for the database |
