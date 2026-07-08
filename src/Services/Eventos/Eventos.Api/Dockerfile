FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.MessageBus/TicketHub.MessageBus.csproj src/BuildingBlocks/TicketHub.MessageBus/
COPY src/Services/Eventos/Eventos.Api/Eventos.Api.csproj src/Services/Eventos/Eventos.Api/
RUN dotnet restore src/Services/Eventos/Eventos.Api/Eventos.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Eventos/Eventos.Api/ src/Services/Eventos/Eventos.Api/
RUN dotnet publish src/Services/Eventos/Eventos.Api/Eventos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Eventos.Api.dll"]
