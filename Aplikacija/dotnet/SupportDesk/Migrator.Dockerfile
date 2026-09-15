FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SupportDesk.WebApi/SupportDesk.WebApi.csproj", "SupportDesk.WebApi/"]
COPY ["SupportDesk.Infrastructure/SupportDesk.Infrastructure.csproj", "SupportDesk.Infrastructure/"]
COPY ["SupportDesk.Application/SupportDesk.Application.csproj", "SupportDesk.Application/"]
COPY ["SupportDesk.Domain/SupportDesk.Domain.csproj", "SupportDesk.Domain/"]

RUN dotnet restore "SupportDesk.WebApi/SupportDesk.WebApi.csproj"

RUN dotnet tool install --global dotnet-ef --version 10.0.*
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .

RUN dotnet ef migrations bundle \
    --project SupportDesk.Infrastructure/SupportDesk.Infrastructure.csproj \
    --startup-project SupportDesk.WebApi/SupportDesk.WebApi.csproj \
    --output /app/bundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/bundle .

ENTRYPOINT ["sh", "-c", "exec ./bundle --connection \"$DB_CONN_STRING\""]