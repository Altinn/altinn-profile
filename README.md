# Altinn Platform Profile

## Build status
[![Profile build status](https://dev.azure.com/brreg/altinn-studio/_apis/build/status/altinn-platform/profile-master?label=platform/profile)](https://dev.azure.com/brreg/altinn-studio/_build/latest?definitionId=35)


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


## Getting Started

These instructions will get you a copy of the profile component up and running on your machine for development and testing purposes.

### Prerequisites

1. [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
2. Code editor of your choice
3. Newest [Git](https://git-scm.com/downloads)
4. [Docker CE](https://www.docker.com/get-docker)
5. Solution is cloned


## Running the profile component

### In a docker container

Clone [Altinn Platform Profile repo](https://github.com/Altinn/altinn-profile) and navigate to the root folder.

```cmd
docker-compose up -d --build
```

### With .NET

The Profile components can be run locally when developing/debugging. Follow the install steps above if this has not already been done.

Stop the container running Profile

```cmd
docker stop altinn-profile
```

Navigate to the src/Profile, and build and run the code from there, or run the solution using you selected code editor

```cmd
dotnet run
```

The profile solution is now available locally at http://localhost:5030/api/v1
