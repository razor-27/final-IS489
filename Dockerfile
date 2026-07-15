# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar los archivos de proyecto para restaurar dependencias
COPY ["WariSalud.API/WariSalud.API.csproj", "WariSalud.API/"]
COPY ["WariSalud.Core/WariSalud.Core.csproj", "WariSalud.Core/"]
COPY ["WariSalud.Infrastructure/WariSalud.Infrastructure.csproj", "WariSalud.Infrastructure/"]

# Restaurar paquetes
RUN dotnet restore "WariSalud.API/WariSalud.API.csproj"

# Copiar todo el código fuente
COPY . .

# Construir y publicar
WORKDIR "/src/WariSalud.API"
RUN dotnet publish "WariSalud.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Render inyecta la variable PORT, ASP.NET Core 8+ usa HTTP_PORTS
ENV ASPNETCORE_HTTP_PORTS=8080

# Copiar los archivos publicados
COPY --from=build /app/publish .

# Iniciar la aplicación
ENTRYPOINT ["dotnet", "WariSalud.API.dll"]
