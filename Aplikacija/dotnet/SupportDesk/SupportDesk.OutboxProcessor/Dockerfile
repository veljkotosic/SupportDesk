FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SupportDesk.OutboxProcessor/SupportDesk.OutboxProcessor.csproj", "SupportDesk.OutboxProcessor/"]
COPY ["SupportDesk.Application/SupportDesk.Application.csproj", "SupportDesk.Application/"]
COPY ["SupportDesk.Domain/SupportDesk.Domain.csproj", "SupportDesk.Domain/"]
COPY ["SupportDesk.Infrastructure/SupportDesk.Infrastructure.csproj", "SupportDesk.Infrastructure/"]

RUN dotnet restore "SupportDesk.OutboxProcessor/SupportDesk.OutboxProcessor.csproj"

COPY . .
WORKDIR /src/SupportDesk.OutboxProcessor
RUN dotnet publish "SupportDesk.OutboxProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5012

ENTRYPOINT ["dotnet", "SupportDesk.OutboxProcessor.dll"]