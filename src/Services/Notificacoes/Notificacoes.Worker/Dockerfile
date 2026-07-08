FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.MessageBus/TicketHub.MessageBus.csproj src/BuildingBlocks/TicketHub.MessageBus/
COPY src/Services/Notificacoes/Notificacoes.Worker/Notificacoes.Worker.csproj src/Services/Notificacoes/Notificacoes.Worker/
RUN dotnet restore src/Services/Notificacoes/Notificacoes.Worker/Notificacoes.Worker.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Notificacoes/Notificacoes.Worker/ src/Services/Notificacoes/Notificacoes.Worker/
RUN dotnet publish src/Services/Notificacoes/Notificacoes.Worker/Notificacoes.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Notificacoes.Worker.dll"]
