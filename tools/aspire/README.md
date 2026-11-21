# Aspire app host

Starts the `profile-db` Postgres container and a WireMock container (`mocks`) for the external dependencies, runs the database migrations, then starts Profile (port 5030), with logs, traces and metrics in the Aspire dashboard.

The mocks and the test certificates are the same as for docker compose, so authenticated calls work with tokens from `local-dev/token.cs`. See [local-dev/README.md](../../local-dev/README.md) for tokens, test data and the mocked services.

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
| WireMock (`mocks`) | `5039`, the same as docker compose |
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
| `Mocks:UseContainer` | `true` | `false` skips WireMock, so Profile uses the endpoints in its `appsettings.json` (locally running Altinn services) |
| `Mocks:Port` | `5039` | Host port for WireMock |

## Access tokens without Key Vault

Profile validates incoming platform access tokens with the public certificates in `GeneralSettings:AccessTokenPublicCertsFolder` when `kvSetting:SecretUri` is not set. The app host copies `test/Altinn.Profile.Tests/ttd-org.pem` to a temporary folder as `platform-org.pem` and `ttd-org.pem`, and signs outgoing platform access tokens with `ttd-org.pfx`.
