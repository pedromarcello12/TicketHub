FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Auth/TicketHub.Auth.csproj src/BuildingBlocks/TicketHub.Auth/
COPY src/Services/Auth/Auth.Api/Auth.Api.csproj src/Services/Auth/Auth.Api/
RUN dotnet restore src/Services/Auth/Auth.Api/Auth.Api.csproj

COPY src/BuildingBlocks/TicketHub.Auth/ src/BuildingBlocks/TicketHub.Auth/
COPY src/Services/Auth/Auth.Api/ src/Services/Auth/Auth.Api/
RUN dotnet publish src/Services/Auth/Auth.Api/Auth.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
