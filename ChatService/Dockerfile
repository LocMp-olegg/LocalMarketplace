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
COPY ChatService/src/LocMp.Chat.Domain/LocMp.Chat.Domain.csproj                                   ChatService/src/LocMp.Chat.Domain/
COPY ChatService/src/LocMp.Chat.Application/LocMp.Chat.Application.csproj                         ChatService/src/LocMp.Chat.Application/
COPY ChatService/src/LocMp.Chat.Infrastructure/LocMp.Chat.Infrastructure.csproj                   ChatService/src/LocMp.Chat.Infrastructure/
COPY ChatService/src/LocMp.Chat.Api/LocMp.Chat.Api.csproj                                         ChatService/src/LocMp.Chat.Api/

RUN dotnet restore ChatService/src/LocMp.Chat.Api/LocMp.Chat.Api.csproj

COPY BuildingBlocks/  BuildingBlocks/
COPY Contracts/       Contracts/
COPY ChatService/     ChatService/

RUN dotnet publish ChatService/src/LocMp.Chat.Api/LocMp.Chat.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocMp.Chat.Api.dll"]
