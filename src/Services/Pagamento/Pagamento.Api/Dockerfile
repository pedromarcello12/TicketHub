FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.Auth/TicketHub.Auth.csproj src/BuildingBlocks/TicketHub.Auth/
COPY src/BuildingBlocks/TicketHub.MessageBus/TicketHub.MessageBus.csproj src/BuildingBlocks/TicketHub.MessageBus/
COPY src/BuildingBlocks/TicketHub.Observabilidade/TicketHub.Observabilidade.csproj src/BuildingBlocks/TicketHub.Observabilidade/
COPY src/Services/Pagamento/Pagamento.Domain/Pagamento.Domain.csproj src/Services/Pagamento/Pagamento.Domain/
COPY src/Services/Pagamento/Pagamento.Application/Pagamento.Application.csproj src/Services/Pagamento/Pagamento.Application/
COPY src/Services/Pagamento/Pagamento.Infrastructure/Pagamento.Infrastructure.csproj src/Services/Pagamento/Pagamento.Infrastructure/
COPY src/Services/Pagamento/Pagamento.Api/Pagamento.Api.csproj src/Services/Pagamento/Pagamento.Api/
RUN dotnet restore src/Services/Pagamento/Pagamento.Api/Pagamento.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Pagamento/ src/Services/Pagamento/
RUN dotnet publish src/Services/Pagamento/Pagamento.Api/Pagamento.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Pagamento.Api.dll"]
