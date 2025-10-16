FROM mcr.microsoft.com/dotnet/sdk:9.0.306-alpine3.22@sha256:f271ed7d0fd9c5a7ed0acafed8a2bc978bb65c19dcd2eeea0415adef142ffc87 AS build
WORKDIR /app

COPY src/Altinn.Profile/*.csproj ./src/Altinn.Profile/
COPY src/Altinn.Profile.Models/*.csproj ./src/Altinn.Profile.Models/
COPY src/Altinn.Profile.Core/*.csproj ./src/Altinn.Profile.Core/
COPY src/Altinn.Profile.Integrations/*.csproj ./src/Altinn.Profile.Integrations/

RUN dotnet restore ./src/Altinn.Profile/Altinn.Profile.csproj

COPY src ./src
RUN dotnet publish -c Release -o /app_output ./src/Altinn.Profile/Altinn.Profile.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0.10-alpine3.22@sha256:5e8dca92553951e42caed00f2568771b0620679f419a28b1335da366477d7f98 AS final
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
