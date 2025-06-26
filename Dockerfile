FROM mcr.microsoft.com/dotnet/sdk:9.0.301-alpine3.22@sha256:bdd1c9e2215a71e43d2f0c6978ace0a0652d7ecc21bf6f659d42d840500e1c44 AS build
WORKDIR /app

COPY src/Altinn.Profile/*.csproj ./src/Altinn.Profile/
COPY src/Altinn.Profile.Core/*.csproj ./src/Altinn.Profile.Core/
COPY src/Altinn.Profile.Integrations/*.csproj ./src/Altinn.Profile.Integrations/

RUN dotnet restore ./src/Altinn.Profile/Altinn.Profile.csproj

COPY src ./src
RUN dotnet publish -c Release -o /app_output ./src/Altinn.Profile/Altinn.Profile.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0.6-alpine3.22@sha256:14f13652a7907d905063a9103731c9244e42cbd2f6c588a2d9666677bab0370b AS final
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
