FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Auth/TicketHub.Auth.csproj src/BuildingBlocks/TicketHub.Auth/
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.Observabilidade/TicketHub.Observabilidade.csproj src/BuildingBlocks/TicketHub.Observabilidade/
COPY src/Gateway/TicketHub.Gateway/TicketHub.Gateway.csproj src/Gateway/TicketHub.Gateway/
RUN dotnet restore src/Gateway/TicketHub.Gateway/TicketHub.Gateway.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Gateway/TicketHub.Gateway/ src/Gateway/TicketHub.Gateway/
RUN dotnet publish src/Gateway/TicketHub.Gateway/TicketHub.Gateway.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "TicketHub.Gateway.dll"]
