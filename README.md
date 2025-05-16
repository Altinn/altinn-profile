# Altinn Profile

## Build status
[![Profile build status](https://dev.azure.com/brreg/altinn-studio/_apis/build/status/altinn-platform/profile-master?label=platform/profile)](https://dev.azure.com/brreg/altinn-studio/_build/latest?definitionId=38)


## Project organization
This is a backend WebAPI solution written in .NET / C# following the clean architecture principles.

### Altinn.Profile
The API layer that consumes services provided by _Altinn.Profile.Core_

Relevant implementations:
- Controllers
- Program.cs


### Altinn.Profile.Core
The domain and application layer that implements the business logic of the system.

Relevant implementations:
- Interfaces for external dependencies implemented by infrastructure and repository layer
- Domain models
- Services

### Altinn.Profile.Integrations
The infrastructure layer that implements the interfaces defined in _Altinn.Profile.Core_ for integrations towards 3rd-party libraries and systems.

Relevant implementations:
- Clients for communicating with SBL Bridge in Altinn 2
- Database for KRR-data
- Client for communicating with KRR to update the local DB


## Getting Started

These instructions will get you a copy of the profile component up and running on your machine for development and testing purposes.

### Prerequisites

1. [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Newest [Git](https://git-scm.com/downloads)
3. A code editor - we like [Visual Studio Code](https://code.visualstudio.com/download)
   - Also install [recommended extensions](https://code.visualstudio.com/docs/editor/extension-marketplace#_workspace-recommended-extensions) (e.g. [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp))
4. [Podman](https://podman.io/) or another container tool such as Docker Desktop
5. [PostgreSQL](https://www.postgresql.org/download/)
6. [pgAdmin](https://www.pgadmin.org/download/)

### Setting up PostgreSQL

Ensure that both PostgreSQL and pgAdmin have been installed and start pgAdmin.

In pgAdmin
- Create database _profiledb_
- Create the following users with password: _Password_ (see privileges in parentheses)
  - platform_profile_admin (superuser, canlogin)
  - platform_profile (canlogin)

A more detailed description of the database setup is available in [our developer handbook](https://docs.altinn.studio/community/contributing/handbook/postgres/)


### Cloning the application

Clone [Altinn Profile repo](https://github.com/Altinn/altinn-profile) and navigate to the folder.

```bash
git clone https://github.com/Altinn/altinn-profile
cd altinn-profile
```

### Running the application in a docker container

- Start Altinn Profile docker container by running the command

```cmd
podman compose up -d --build
```

- To stop the container running Altinn Profile run the command

```cmd
podman stop altinn-platform-profile
```


### Running the application with .NET

The Profile components can be run locally when developing/debugging. Follow the install steps above if this has not already been done.

- Navigate to _src/Altinn.Profile, and build and run the code from there, or run the solution using you selected code editor

```cmd
cd src/Altinn.Profile
dotnet run
```

The profile solution is now available locally at http://localhost:5030/.
To access swagger use http://localhost:5030/swagger.

### Populate the Profile DB with KRR data

1. Set up required user secrets for Maskinporten integration in the ASP.NET Core Secret Manager, e.g. via CLI by running the following commands from `src/Altinn.Profile`
```cmd
dotnet user-secrets set "ContactAndReservationSettings:MaskinportenSettings:ClientId" "{SECRET_GOES_HERE}"
dotnet user-secrets set "ContactAndReservationSettings:MaskinportenSettings:EncodedJwk" "{SECRET_GOES_HERE}"
```
2. Run the application, and send the following request (e.g. using Postman) to initiate the synchronization job:
   ```cmd
   GET http://localhost:5030/profile/api/v1/trigger/syncpersonchanges
   ```

### Adding migrations with EF core
1. Code the desired classes and add them to the DB context
2. Run the following command in a terminal from src/Altinn.Profile.Integrations to add a migration. The name should be more descriptive than AddNewMigration:
   ```cmd
   dotnet ef migrations Add AddNewMigration --startup-project ../Altinn.Profile
   ```
3. To generate SQL scripts, run `dotnet ef migrations script --startup-project ../Altinn.Profile`

If you want to remove the migration run `dotnet ef migrations remove --startup-project ../Altinn.Profile`
Read more about applying migrations [here](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli).

### Running Bruno requests
In order to run the requests with active access tokens, you need to add the secrets for the token generator tool. This should be added in a `.env` file. An example is added to the folder. 