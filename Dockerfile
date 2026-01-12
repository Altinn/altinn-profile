FROM mcr.microsoft.com/dotnet/sdk:10.0.101-alpine3.23@sha256:9698eeadcdb786aafd10802e9196dd918c2aed7af9a7297a33ae272123d96bc9 AS build
WORKDIR /app

COPY src/Altinn.Profile/*.csproj ./src/Altinn.Profile/
COPY src/Altinn.Profile.Models/*.csproj ./src/Altinn.Profile.Models/
COPY src/Altinn.Profile.Core/*.csproj ./src/Altinn.Profile.Core/
COPY src/Altinn.Profile.Integrations/*.csproj ./src/Altinn.Profile.Integrations/

RUN dotnet restore ./src/Altinn.Profile/Altinn.Profile.csproj

COPY src ./src
RUN dotnet publish -c Release -o /app_output ./src/Altinn.Profile/Altinn.Profile.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.23@sha256:a126f0550b963264c5493fd9ec5dc887020c7ea7ebe1da09bc10c9f26d16c253 AS final
EXPOSE 5030
WORKDIR /app

COPY --from=build /app_output .
COPY --from=build /app/src/Altinn.Profile.Integrations/Migration ./Migration

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet

# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry

ENTRYPOINT ["dotnet", "Altinn.Profile.dll"]
