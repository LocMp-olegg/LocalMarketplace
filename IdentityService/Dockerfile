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
COPY IdentityService/src/LocMp.Identity.Domain/LocMp.Identity.Domain.csproj                       IdentityService/src/LocMp.Identity.Domain/
COPY IdentityService/src/LocMp.Identity.Application/LocMp.Identity.Application.csproj             IdentityService/src/LocMp.Identity.Application/
COPY IdentityService/src/LocMp.Identity.Infrastructure/LocMp.Identity.Infrastructure.csproj       IdentityService/src/LocMp.Identity.Infrastructure/
COPY IdentityService/src/LocMp.Identity.Api/LocMp.Identity.Api.csproj                             IdentityService/src/LocMp.Identity.Api/

RUN dotnet restore IdentityService/src/LocMp.Identity.Api/LocMp.Identity.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY IdentityService/ IdentityService/

RUN dotnet publish IdentityService/src/LocMp.Identity.Api/LocMp.Identity.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Identity.Api.dll"]
