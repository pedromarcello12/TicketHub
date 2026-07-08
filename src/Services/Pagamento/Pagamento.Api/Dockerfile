FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.MessageBus/TicketHub.MessageBus.csproj src/BuildingBlocks/TicketHub.MessageBus/
COPY src/Services/Pagamento/Pagamento.Api/Pagamento.Api.csproj src/Services/Pagamento/Pagamento.Api/
RUN dotnet restore src/Services/Pagamento/Pagamento.Api/Pagamento.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Pagamento/Pagamento.Api/ src/Services/Pagamento/Pagamento.Api/
RUN dotnet publish src/Services/Pagamento/Pagamento.Api/Pagamento.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pagamento.Api.dll"]
