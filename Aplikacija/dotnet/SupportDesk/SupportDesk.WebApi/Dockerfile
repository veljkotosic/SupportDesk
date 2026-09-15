FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SupportDesk.WebApi/SupportDesk.WebApi.csproj", "SupportDesk.WebApi/"]
COPY ["SupportDesk.Application/SupportDesk.Application.csproj", "SupportDesk.Application/"]
COPY ["SupportDesk.Domain/SupportDesk.Domain.csproj", "SupportDesk.Domain/"]
COPY ["SupportDesk.Infrastructure/SupportDesk.Infrastructure.csproj", "SupportDesk.Infrastructure/"]

RUN dotnet restore "SupportDesk.WebApi/SupportDesk.WebApi.csproj"

COPY . .
WORKDIR /src/SupportDesk.WebApi
RUN dotnet publish "SupportDesk.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SupportDesk.WebApi.dll"]