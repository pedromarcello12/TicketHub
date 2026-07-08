FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TicketHub.slnx .
COPY src/BuildingBlocks/TicketHub.Core/TicketHub.Core.csproj src/BuildingBlocks/TicketHub.Core/
COPY src/BuildingBlocks/TicketHub.MessageBus/TicketHub.MessageBus.csproj src/BuildingBlocks/TicketHub.MessageBus/
COPY src/Services/Ingressos/Ingressos.Domain/Ingressos.Domain.csproj src/Services/Ingressos/Ingressos.Domain/
COPY src/Services/Ingressos/Ingressos.Application/Ingressos.Application.csproj src/Services/Ingressos/Ingressos.Application/
COPY src/Services/Ingressos/Ingressos.Infrastructure/Ingressos.Infrastructure.csproj src/Services/Ingressos/Ingressos.Infrastructure/
COPY src/Services/Ingressos/Ingressos.Api/Ingressos.Api.csproj src/Services/Ingressos/Ingressos.Api/
RUN dotnet restore src/Services/Ingressos/Ingressos.Api/Ingressos.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Ingressos/ src/Services/Ingressos/
RUN dotnet publish src/Services/Ingressos/Ingressos.Api/Ingressos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Ingressos.Api.dll"]
