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
COPY CatalogService/src/LocMp.Catalog.Domain/LocMp.Catalog.Domain.csproj                          CatalogService/src/LocMp.Catalog.Domain/
COPY CatalogService/src/LocMp.Catalog.Application/LocMp.Catalog.Application.csproj                CatalogService/src/LocMp.Catalog.Application/
COPY CatalogService/src/LocMp.Catalog.Infrastructure/LocMp.Catalog.Infrastructure.csproj          CatalogService/src/LocMp.Catalog.Infrastructure/
COPY CatalogService/src/LocMp.Catalog.Api/LocMp.Catalog.Api.csproj                                CatalogService/src/LocMp.Catalog.Api/

RUN dotnet restore CatalogService/src/LocMp.Catalog.Api/LocMp.Catalog.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY CatalogService/  CatalogService/

RUN dotnet publish CatalogService/src/LocMp.Catalog.Api/LocMp.Catalog.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Catalog.Api.dll"]
