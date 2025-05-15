FROM mcr.microsoft.com/dotnet/sdk:9.0.203-alpine3.21@sha256:6fc61b57f8fa5f333e30c4192076fbd8b76f90dd2c5b2e5f9066f76f5b726832 AS build
WORKDIR /app

COPY src/Altinn.Profile/*.csproj ./src/Altinn.Profile/
COPY src/Altinn.Profile.Core/*.csproj ./src/Altinn.Profile.Core/
COPY src/Altinn.Profile.Integrations/*.csproj ./src/Altinn.Profile.Integrations/

RUN dotnet restore ./src/Altinn.Profile/Altinn.Profile.csproj

COPY src ./src
RUN dotnet publish -c Release -o /app_output ./src/Altinn.Profile/Altinn.Profile.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0.4-alpine3.21@sha256:3fce6771d84422e2396c77267865df61174a3e503c049f1fe242224c012fde65 AS final
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
