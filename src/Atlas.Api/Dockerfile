# Etapa 1: Base para ejecución (Ligera)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
# Parche de seguridad en tiempo de compilación para mitigar CVEs del SO base
RUN apt-get update && apt-get upgrade -y && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 80

# Etapa 2: Construcción (Contiene el SDK pesado)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copiamos solo el csproj primero para aprovechar el caché de Docker
COPY ["Atlas.Api.csproj", "./"]
RUN dotnet restore "Atlas.Api.csproj"
# Ahora copiamos el resto del código fuente
COPY . .
RUN dotnet build "Atlas.Api.csproj" -c Release -o /app/build

# Etapa 3: Publicación
FROM build AS publish
RUN dotnet publish "Atlas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 4: Imagen Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Atlas.Api.dll"]