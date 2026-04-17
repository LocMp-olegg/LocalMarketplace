FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY BuildingBlocks/src/LocMp.BuildingBlocks.Domain/LocMp.BuildingBlocks.Domain.csproj            BuildingBlocks/src/LocMp.BuildingBlocks.Domain/
COPY BuildingBlocks/src/LocMp.BuildingBlocks.Application/LocMp.BuildingBlocks.Application.csproj  BuildingBlocks/src/LocMp.BuildingBlocks.Application/
COPY BuildingBlocks/src/LocMp.BuildingBlocks.Infrastructure/LocMp.BuildingBlocks.Infrastructure.csproj BuildingBlocks/src/LocMp.BuildingBlocks.Infrastructure/
COPY Contracts/LocMp.Contracts/LocMp.Contracts.csproj                                             Contracts/LocMp.Contracts/
COPY AnalyticsService/src/LocMp.Analytics.Domain/LocMp.Analytics.Domain.csproj                   AnalyticsService/src/LocMp.Analytics.Domain/
COPY AnalyticsService/src/LocMp.Analytics.Application/LocMp.Analytics.Application.csproj         AnalyticsService/src/LocMp.Analytics.Application/
COPY AnalyticsService/src/LocMp.Analytics.Infrastructure/LocMp.Analytics.Infrastructure.csproj   AnalyticsService/src/LocMp.Analytics.Infrastructure/
COPY AnalyticsService/src/LocMp.Analytics.Api/LocMp.Analytics.Api.csproj                         AnalyticsService/src/LocMp.Analytics.Api/

RUN dotnet restore AnalyticsService/src/LocMp.Analytics.Api/LocMp.Analytics.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY AnalyticsService/ AnalyticsService/

RUN dotnet publish AnalyticsService/src/LocMp.Analytics.Api/LocMp.Analytics.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Analytics.Api.dll"]
