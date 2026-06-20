FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar nuget.config y el feed de NuGet local
COPY nuget.config ./
COPY LocalNuGetFeed/ ./LocalNuGetFeed/

# Copiar csproj y restaurar dependencias
COPY src/InventoryService/InventoryService.csproj ./src/InventoryService/
RUN dotnet restore src/InventoryService/InventoryService.csproj

# Copiar el código fuente y publicar
COPY src/InventoryService/ ./src/InventoryService/
RUN dotnet publish src/InventoryService/InventoryService.csproj -c Release -o out

# Imagen de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "InventoryService.dll"]
