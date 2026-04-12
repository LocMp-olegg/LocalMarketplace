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
COPY OrderService/src/LocMp.Order.Domain/LocMp.Order.Domain.csproj                                OrderService/src/LocMp.Order.Domain/
COPY OrderService/src/LocMp.Order.Application/LocMp.Order.Application.csproj                      OrderService/src/LocMp.Order.Application/
COPY OrderService/src/LocMp.Order.Infrastructure/LocMp.Order.Infrastructure.csproj                OrderService/src/LocMp.Order.Infrastructure/
COPY OrderService/src/LocMp.Order.Api/LocMp.Order.Api.csproj                                      OrderService/src/LocMp.Order.Api/

RUN dotnet restore OrderService/src/LocMp.Order.Api/LocMp.Order.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY OrderService/    OrderService/

RUN dotnet publish OrderService/src/LocMp.Order.Api/LocMp.Order.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Order.Api.dll"]
