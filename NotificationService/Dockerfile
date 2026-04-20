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
COPY NotificationService/src/LocMp.Notification.Domain/LocMp.Notification.Domain.csproj           NotificationService/src/LocMp.Notification.Domain/
COPY NotificationService/src/LocMp.Notification.Application/LocMp.Notification.Application.csproj NotificationService/src/LocMp.Notification.Application/
COPY NotificationService/src/LocMp.Notification.Infrastructure/LocMp.Notification.Infrastructure.csproj NotificationService/src/LocMp.Notification.Infrastructure/
COPY NotificationService/src/LocMp.Notification.Api/LocMp.Notification.Api.csproj                 NotificationService/src/LocMp.Notification.Api/

RUN dotnet restore NotificationService/src/LocMp.Notification.Api/LocMp.Notification.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY NotificationService/ NotificationService/

RUN dotnet publish NotificationService/src/LocMp.Notification.Api/LocMp.Notification.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Notification.Api.dll"]
