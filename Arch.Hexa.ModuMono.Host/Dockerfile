
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Arch.Hexa.ModuMono.RestApi/Arch.Hexa.ModuMono.RestApi.csproj", "Arch.Hexa.ModuMono.RestApi/"]
COPY ["Arch.Hexa.ModuMono.BuildingBlocks.Application/Arch.Hexa.ModuMono.BuildingBlocks.Application.csproj", "Arch.Hexa.ModuMono.BuildingBlocks.Application/"]
COPY ["Arch.Hexa.ModuMono.BuildingBlocks.Domain/Arch.Hexa.ModuMono.BuildingBlocks.Domain.csproj", "Arch.Hexa.ModuMono.BuildingBlocks.Domain/"]
COPY ["Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure/Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.csproj", "Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure/"]
COPY ["Arch.Hexa.ModuMono.Modules.Customers.Application/Arch.Hexa.ModuMono.Modules.Customers.Application.csproj", "Arch.Hexa.ModuMono.Modules.Customers.Application/"]
COPY ["Arch.Hexa.ModuMono.Modules.Customers.Domain/Arch.Hexa.ModuMono.Modules.Customers.Domain.csproj", "Arch.Hexa.ModuMono.Modules.Customers.Domain/"]
COPY ["Arch.Hexa.ModuMono.Modules.Customers.Infrastructure/Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.csproj", "Arch.Hexa.ModuMono.Modules.Customers.Infrastructure/"]
COPY ["Arch.Hexa.ModuMono.Modules.Orders.Application/Arch.Hexa.ModuMono.Modules.Orders.Application.csproj", "Arch.Hexa.ModuMono.Modules.Orders.Application/"]
COPY ["Arch.Hexa.ModuMono.Modules.Orders.Domain/Arch.Hexa.ModuMono.Modules.Orders.Domain.csproj", "Arch.Hexa.ModuMono.Modules.Orders.Domain/"]
COPY ["Arch.Hexa.ModuMono.Modules.Orders.Infrastructure/Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.csproj", "Arch.Hexa.ModuMono.Modules.Orders.Infrastructure/"]
RUN dotnet restore "./Arch.Hexa.ModuMono.RestApi/Arch.Hexa.ModuMono.RestApi.csproj"
COPY . .
WORKDIR "/src/Arch.Hexa.ModuMono.RestApi"
RUN dotnet build "./Arch.Hexa.ModuMono.RestApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Arch.Hexa.ModuMono.RestApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Architecture.Pocs.ModularMonolith.RestApi.dll"]