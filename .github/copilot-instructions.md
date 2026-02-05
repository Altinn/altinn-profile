# Altinn Profile - Copilot Instructions

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
- Async methods: `Async` suffix convention
- Using directives: Outside namespace, system directives first

### StyleCop Rules
Strict enforcement (most rules are errors/warnings). Notable:
- SA1600/SA1601: XML documentation required (errors)
- SA1309: Field names not prefixed with underscore (disabled - we use `_` prefix)
- SA1101: Prefix local calls with this (disabled)
- SA1200: Using directives placement (disabled - handled by .editorconfig)
- SA1633-SA1643: File headers (disabled)

### Patterns
- **Result pattern**: Use `Result<T>` from Core for operation outcomes
- **Optional pattern**: Use `Optional<T>` from Core for nullable value semantics with JSON converter support
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
