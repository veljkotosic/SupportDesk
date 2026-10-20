FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SupportDesk.Worker/SupportDesk.Worker.csproj", "SupportDesk.Worker/"]
COPY ["SupportDesk.Application/SupportDesk.Application.csproj", "SupportDesk.Application/"]
COPY ["SupportDesk.Domain/SupportDesk.Domain.csproj", "SupportDesk.Domain/"]
COPY ["SupportDesk.Infrastructure/SupportDesk.Infrastructure.csproj", "SupportDesk.Infrastructure/"]

RUN dotnet restore "SupportDesk.Worker/SupportDesk.Worker.csproj"

COPY . .
WORKDIR /src/SupportDesk.Worker
RUN dotnet publish "SupportDesk.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5013

ENTRYPOINT ["dotnet", "SupportDesk.Worker.dll"]