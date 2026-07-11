FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.Auth/TicketHub.Auth.csproj src/BuildingBlocks/TicketHub.Auth/
COPY src/Services/Auth/Auth.Domain/Auth.Domain.csproj src/Services/Auth/Auth.Domain/
COPY src/Services/Auth/Auth.Application/Auth.Application.csproj src/Services/Auth/Auth.Application/
COPY src/Services/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj src/Services/Auth/Auth.Infrastructure/
COPY src/Services/Auth/Auth.Api/Auth.Api.csproj src/Services/Auth/Auth.Api/
RUN dotnet restore src/Services/Auth/Auth.Api/Auth.Api.csproj

COPY src/BuildingBlocks/TicketHub.Core/ src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.Auth/ src/BuildingBlocks/TicketHub.Auth/
COPY src/Services/Auth/ src/Services/Auth/
RUN dotnet publish src/Services/Auth/Auth.Api/Auth.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
