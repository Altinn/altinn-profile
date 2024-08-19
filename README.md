# Altinn Profile

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

1. [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Newest [Git](https://git-scm.com/downloads)
3. A code editor - we like [Visual Studio Code](https://code.visualstudio.com/download)
   - Also install [recommended extensions](https://code.visualstudio.com/docs/editor/extension-marketplace#_workspace-recommended-extensions) (e.g. [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp))
4. [Podman](https://podman.io/) or another container tool such as Docker Desktop


### Cloning the application

Clone [Altinn Profile repo](https://github.com/Altinn/altinn-profile) and navigate to the folder.

```bash
git clone https://github.com/Altinn/altinn-profile
cd altinn-profile
```

### Running the application in a docker container

- Start Altinn Profile docker container run the command

```cmd
podman compose up -d --build
```

- To stop the container running Altinn Profile run the command

```cmd
podman stop altinn-profile
```


### Running the application with .NET

The Profile components can be run locally when developing/debugging. Follow the install steps above if this has not already been done.

- Navigate to _src/Profile, and build and run the code from there, or run the solution using you selected code editor

```cmd
cd src/Profile
dotnet run
```

The profile solution is now available locally at http://localhost:5030/.
To access swagger use http://localhost:5030/swagger.
