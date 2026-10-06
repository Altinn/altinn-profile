# Running Altinn Profile locally with Docker

The `docker-compose.yml` in the repository root starts a self-contained environment, with no
dependencies on other Altinn services or Azure:

| Service      | What it is                                                                 | Host port                         |
|--------------|-----------------------------------------------------------------------------|-----------------------------------|
| `postgres`   | PostgreSQL 17 with `profiledb`, `platform_profile_admin` and `platform_profile` | `127.0.0.1:5435` (`PROFILE_DB_PORT`) |
| `migrations` | Runs the EF Core migrations (`--run-db-migrations`) and exits               | –                                 |
| `profile`    | The Profile API, built from the root `Dockerfile`                           | `127.0.0.1:5030` (`PROFILE_API_PORT`) |
| `mocks`      | WireMock, standing in for every external dependency (see below)            | `127.0.0.1:5039` (`PROFILE_MOCKS_PORT`) |

```bash
docker compose up -d --build     # start (rebuilds the API image)
docker compose logs -f profile   # follow the API log
docker compose down              # stop, keep the database
docker compose down -v           # stop and delete the database
```

Swagger: http://localhost:5030/swagger

Ports can be changed with the environment variables in the table, e.g. `PROFILE_DB_PORT=5440 docker compose up -d`.

## Tokens

Authenticated endpoints need a JWT. `token.cs` is a .NET 10 file-based app that creates one (requires the .NET 10 SDK):

```bash
# End user (the test person, user 20462603 / party 50891883), scope altinn:portal/enduser
dotnet run local-dev/token.cs

# Other users or scopes
dotnet run local-dev/token.cs -- user --user-id 20462603 --party-id 50891883 --scope "altinn:portal/enduser"

# Organization / Maskinporten-style token
dotnet run local-dev/token.cs -- org --org-number 313605590 --scope "altinn:profile.support.admin"

# PlatformAccessToken, for endpoints that require one (header "PlatformAccessToken")
dotnet run local-dev/token.cs -- platform --app notifications
```

```bash
TOKEN=$(dotnet run local-dev/token.cs)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5030/profile/api/v1/users/current
```

How it fits together:

- User and org tokens are signed with `test/Altinn.Profile.Tests/jwtselfsignedcert.pfx`. The `mocks` container
  publishes its public key as the OpenID configuration and JWKS that `GeneralSettings:OpenIdWellKnownEndpoint` points to.
- Platform access tokens are signed with `test/Altinn.Profile.Tests/ttd-org.pfx`. Without Key Vault, Profile reads the
  public certificates from `GeneralSettings:AccessTokenPublicCertsFolder` (`{issuer}-org.pem`), and compose mounts
  `ttd-org.pem` there as both `platform-org.pem` and `ttd-org.pem`.
- Outgoing platform access tokens (to Register and Notifications) are signed with the same `ttd-org.pfx`.

## Mocked dependencies

All mocks live in `wiremock/mappings`, with larger response bodies in `wiremock/__files`.

| Dependency                       | Mock behaviour                                                                                   | File                 |
|----------------------------------|--------------------------------------------------------------------------------------------------|----------------------|
| Platform authentication (OIDC)  | OpenID configuration and JWKS for the test certificate                                           | `oidc.json`          |
| Authorization (PDP)              | Every decision is `Permit`, and every user may act for every party                               | `authorization.json` |
| Register                         | Knows one person and one organization (below). Unknown parties return empty results.             | `register.json`      |
| Notifications                    | Accepts instant SMS and email orders                                                             | `notifications.json` |
| Brreg KoF (org notification addresses) | Accepts create/update/delete. The change log is empty.                                     | `brreg.json`         |

Not mocked, because they are off by default: the KRR sync (`JobSettings:KrrSyncEnabled`, needs Maskinporten) and the
Brreg org sync (`JobSettings:OrgSyncEnabled`).

### Test data

| What               | Values                                                                                        |
|--------------------|-----------------------------------------------------------------------------------------------|
| Person             | user id `20462603`, party id `50891883`, party uuid `629fa2c0-27cd-40a2-ac6a-99bdf374dba2`, national identity number `17902349936` |
| Organization       | org number `313605590`, party id `51118947`, party uuid `7aa1d1e2-0f4b-4c55-9b0a-3f5e0c2d9a11` |

### Reading verification codes

Address verification codes are sent through the Notifications mock. Read them from the WireMock request journal:

```bash
curl -s "http://localhost:5039/__admin/requests" | grep -o 'Oppgi koden [0-9]*'
```

### Adding or changing mock responses

Edit or add JSON files in `wiremock/mappings` (see the [WireMock stubbing docs](https://wiremock.org/docs/stubbing/)),
then run `docker compose restart mocks`. Mappings with a lower `priority` number win, which is how the specific
Register stubs take precedence over the "unknown party" fallbacks.
