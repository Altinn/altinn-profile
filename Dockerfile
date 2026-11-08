FROM mcr.microsoft.com/dotnet/sdk:10.0.402-alpine3.24@sha256:230c3fbae0ceedc70bca04eed2bfb5694e8b248cadbb81010f41b7a993601f55 AS build
WORKDIR /app

COPY src/Altinn.Profile/*.csproj ./src/Altinn.Profile/
COPY src/Altinn.Profile.Models/*.csproj ./src/Altinn.Profile.Models/
COPY src/Altinn.Profile.Core/*.csproj ./src/Altinn.Profile.Core/
COPY src/Altinn.Profile.Integrations/*.csproj ./src/Altinn.Profile.Integrations/

RUN dotnet restore ./src/Altinn.Profile/Altinn.Profile.csproj

COPY src ./src
RUN dotnet publish -c Release -o /app_output ./src/Altinn.Profile/Altinn.Profile.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-alpine3.24@sha256:254fe017495996d3abcc487ffacdcce2b73affb380d3af87af12c8dff41fe0b0 AS final
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
