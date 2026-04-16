# Altinn Profile - AI Agent Instructions

## Project Overview

Backend WebAPI solution for user profile management in the Altinn platform. Built with .NET 10/C# following clean architecture principles with three main layers:

- **Altinn.Profile** - API layer (controllers, Program.cs)
- **Altinn.Profile.Core** - Domain/application layer (interfaces, domain models, services)
- **Altinn.Profile.Integrations** - Infrastructure layer (clients for SBL Bridge, database for KRR data, KRR client)

Additional projects:
- **Altinn.Profile.Models** - Shared DTOs
- **ServiceDefaults.Jobs** - Background job infrastructure
- **ServiceDefaults.Leases** - Distributed locking mechanism

## Build, Test, and Lint Commands

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Altinn.Profile
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/Altinn.Profile.Tests

# Run a single test by filter
dotnet test --filter "FullyQualifiedName~Altinn.Profile.Tests.Profile.Core.User.UserContactPointServiceTest"
```

### Linting
StyleCop analyzers run automatically during `dotnet build` in Debug configuration. Configuration is in:
- `stylecop.json` - Using directives outside namespace, system using directives first
- `.editorconfig` - Private fields with `_` prefix, extensive StyleCop rules (mostly warnings/errors)

No separate lint command needed.

### Running Locally
```bash
# With .NET (requires PostgreSQL running locally on port 5432)
cd src/Altinn.Profile
dotnet run
# Access at http://localhost:5030 or http://localhost:5030/swagger

# With Docker/Podman
podman compose up -d --build
podman stop altinn-platform-profile
```

## Architecture & Patterns

### Clean Architecture Layers
- **API → Core → Integrations** dependency flow
- Core defines interfaces; Integrations implements them
- Domain models in Core; DTOs in Altinn.Profile.Models

### Key Integrations
- **SBL Bridge** - Communication with Altinn 2 (user profiles, favorites, settings, changelog)
- **KRR (Contact and Reservation Register)** - Person contact details sync via Maskinporten
- **Register** - Organization/party lookups
- **Notifications** - Email/SMS order client
- **Authorization** - PDP/PEP integration for access control

### Background Jobs
Jobs use `ServiceDefaults.Jobs` infrastructure with lease-based locking via `ServiceDefaults.Leases`:
- `ContactRegisterUpdateJob` - Syncs KRR data
- `OrganizationNotificationAddressUpdateJob` - Syncs organization notification addresses
- `FavoriteImportJob` - Imports favorites from Altinn 2
- `NotificationSettingsImportJob` - Imports notification settings
- `ProfileSettingImportJob` - Imports profile settings

Jobs inherit from `Job` base class and implement `IJob`. Leases prevent concurrent execution.

### Database Migrations
Uses Entity Framework Core with PostgreSQL:
```bash
# Add migration (run from src/Altinn.Profile.Integrations)
dotnet ef migrations Add <MigrationName> --startup-project ../Altinn.Profile

# Generate SQL script
dotnet ef migrations script --startup-project ../Altinn.Profile

# Remove last migration
dotnet ef migrations remove --startup-project ../Altinn.Profile
```

After adding migration, copy generated SQL to new version folder under `Altinn.Profile.Integrations/Migration` for Yuniql deployment.

### Authorization
Uses Altinn PEP (Policy Enforcement Point) with custom handlers:
- `PartyAccessHandler` - Validates party access
- `OrgResourceAccessHandler` - Validates organization resource access
- Claims extracted via `ClaimsHelper` from JWT tokens

## Code Conventions

### Naming
- Private fields: `_camelCase` prefix (enforced by .editorconfig)
- Interfaces: `I` prefix
- Using directives: Outside namespace, system directives first

### Language
- All code, comments, and XML documentation must be written in English

### XML Documentation
All public types and members require `<summary>` docs (and `<param>`/`<returns>` where applicable) — SA1600/SA1601 are build errors. This is a common oversight in AI-generated code.

### StyleCop Rules
Strict enforcement (most rules are errors/warnings). Notable:
- SA1600/SA1601: XML documentation required on all public members (errors — see above)
- SA1309: Field names not prefixed with underscore (disabled - we use `_` prefix)
- SA1101: Prefix local calls with this (disabled)
- SA1200: Using directives placement (disabled - handled by .editorconfig)
- SA1633-SA1643: File headers (disabled)

### Patterns
- **Repository pattern**: Core defines `I*Repository` interfaces; Integrations implements with EF Core
- **Service decorators**: See `UserProfileCachingDecorator` for caching behavior
- **Event handlers**: Domain events handled in Integrations (e.g., `FavoriteAddedEventHandler`)
- **Transactional outbox**: `EFCoreTransactionalOutbox` ensures reliable event publishing

### Test Structure
Test project mirrors src structure:
- `IntegrationTests/` - Full API tests with `ProfileWebApplicationFactory`
- `Profile.Core/` - Core logic tests (services, utilities)
- `Profile.Integrations/` - Integration layer tests (repositories, clients, handlers)
- `Profile/` - API layer tests (validators, mappers)

Uses xUnit, Moq for mocking. Integration tests use in-memory test doubles and custom factories.

### GitHub workflows
- When creating new workflows or introducing the new jobs or steps, follow the principle of least privilege in GITHUB_TOKEN permission grants for the build workflow jobs. Do so by using `permissions` to modify the default permissions granted to the GITHUB_TOKEN, so that you only allow the minimum required access to the workflow jobs. Always set permissions on the workflow-level, but only specify the permissions that _all_ jobs require. If there are no shared permission requirements among the jobs, set `permissions: {}` at the workflow-level to to avoid unwanted inheritance of workflow-level permission grants to jobs that don't require them. When there are some jobs that require special permissions, set a job-level `permissions` block. Be aware that all unspecified permissions are set to `none` when you specify `permissions` at the job level (i.e., that job won't inherit any grants from the workflow-level permissions).
  Examples:
  - A workflow has jobs A (needs `contents: read`) and B (needs `contents: read` and `contents: write`). Acceptable solutions are:
    - (a) Setting `contents: read` in the workflow permissions; setting a permissions block with `contents: read` and `contents: write` on job B
    - (b) Setting `{}` in the workflow permissions; setting a permissions block with `contents: read` on job A; setting a permissions block with `contents: read` and `contents: write` on job B
  - A workflow has jobs A (needs no permissions) and B (needs `contents: read`). The workflow should have `permissions: {}` at the top level, and job B should have its own `permissions` with `contents: read`.

## Branching and Commit Conventions

- Use **feature branches** for all work: `feature/<short-description>` or `feature/<issue-id>-<short-description>`
- Merges to `main` use **squash commits** — write the squash message as a single, descriptive sentence summarizing the change
- Branch names and commit messages should be in English
- Do not commit directly to `main`

## What NOT to Do

- **Do not modify `ServiceDefaults.Jobs` or `ServiceDefaults.Leases` projects directly** — these are shared infrastructure; changes require separate consideration
- **Do not suppress StyleCop warnings with `#pragma warning disable`** — fix the underlying issue instead
- **Do not add EF Core migrations without also copying the generated SQL** to a new version folder under `Altinn.Profile.Integrations/Migration/` for Yuniql deployment (see Database Migrations section)
- **Do not reference internal implementation details across layer boundaries** — respect the API → Core → Integrations dependency flow; Core must not reference Integrations

## Test Expectations

- All new features and bug fixes must include tests; PRs without tests will not be accepted
- Aim for **at least 80% test coverage** on new code
- Follow the existing test structure (unit tests for services/utilities, integration tests for API endpoints and repositories)
- Use Moq for mocking dependencies; use `ProfileWebApplicationFactory` for full API integration tests
- Tests must pass locally before submitting a PR (`dotnet test`)

## Development Workflow

### Local Database Setup
1. Create PostgreSQL database `profiledb`
2. Create users: `platform_profile_admin` (superuser), `platform_profile` (both with password: Password)
3. Connection strings in `appsettings.Development.json` or user secrets
4. See [developer handbook](https://docs.altinn.studio/community/contributing/handbook/postgres/) for details

### Secrets Management
Use ASP.NET Core Secret Manager for sensitive data:
```bash
# From src/Altinn.Profile
dotnet user-secrets set "ContactAndReservationSettings:MaskinportenSettings:ClientId" "{SECRET}"
dotnet user-secrets set "ContactAndReservationSettings:MaskinportenSettings:EncodedJwk" "{SECRET}"
```

### Triggering Jobs Manually
```bash
# Sync KRR data
GET http://localhost:5030/profile/api/v1/trigger/syncpersonchanges
```

### API Testing
Bruno requests available in `test/Bruno/` directory. Requires secrets in `.env` file for token generation.

## Important Files
- `src/Altinn.Profile/Program.cs` - DI configuration, middleware pipeline
- `src/Altinn.Profile.Integrations/Persistence/ProfiledbContext.cs` - EF Core context
- `src/Altinn.Profile.Integrations/ServiceCollectionExtensions.cs` - Infrastructure DI setup
- `src/Altinn.Profile.Core/Extensions/ServiceCollectionExtensions.cs` - Core DI setup
- `stylecop.json` + `.editorconfig` - Code style enforcement
