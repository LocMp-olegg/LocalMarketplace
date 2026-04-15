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
COPY ReviewService/src/LocMp.Review.Domain/LocMp.Review.Domain.csproj                             ReviewService/src/LocMp.Review.Domain/
COPY ReviewService/src/LocMp.Review.Application/LocMp.Review.Application.csproj                   ReviewService/src/LocMp.Review.Application/
COPY ReviewService/src/LocMp.Review.Infrastructure/LocMp.Review.Infrastructure.csproj             ReviewService/src/LocMp.Review.Infrastructure/
COPY ReviewService/src/LocMp.Review.Api/LocMp.Review.Api.csproj                                   ReviewService/src/LocMp.Review.Api/

RUN dotnet restore ReviewService/src/LocMp.Review.Api/LocMp.Review.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY ReviewService/   ReviewService/

RUN dotnet publish ReviewService/src/LocMp.Review.Api/LocMp.Review.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Review.Api.dll"]
