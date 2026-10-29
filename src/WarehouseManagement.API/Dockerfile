FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/WarehouseManagement.API/WarehouseManagement.API.csproj", "src/WarehouseManagement.API/"]
COPY ["src/WarehouseManagement.Application/WarehouseManagement.Application.csproj", "src/WarehouseManagement.Application/"]
COPY ["src/WarehouseManagement.Domain/WarehouseManagement.Domain.csproj", "src/WarehouseManagement.Domain/"]
COPY ["src/WarehouseManagement.Infrastructure/WarehouseManagement.Infrastructure.csproj", "src/WarehouseManagement.Infrastructure/"]
RUN dotnet restore "src/WarehouseManagement.API/WarehouseManagement.API.csproj"
COPY . .
WORKDIR "/src/src/WarehouseManagement.API"
RUN dotnet build "WarehouseManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WarehouseManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WarehouseManagement.API.dll"]
