FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Gateway/src/LocMp.Gateway.Api/LocMp.Gateway.Api.csproj Gateway/src/LocMp.Gateway.Api/

RUN dotnet restore Gateway/src/LocMp.Gateway.Api/LocMp.Gateway.Api.csproj

COPY Gateway/ Gateway/

RUN dotnet publish Gateway/src/LocMp.Gateway.Api/LocMp.Gateway.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Gateway.Api.dll"]
