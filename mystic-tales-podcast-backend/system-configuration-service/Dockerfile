# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["SystemConfigurationService.API/SystemConfigurationService.API.csproj", "SystemConfigurationService.API/"]
COPY ["SystemConfigurationService.BusinessLogic/SystemConfigurationService.BusinessLogic.csproj", "SystemConfigurationService.BusinessLogic/"]
COPY ["SystemConfigurationService.Common/SystemConfigurationService.Common.csproj", "SystemConfigurationService.Common/"]
COPY ["SystemConfigurationService.DataAccess/SystemConfigurationService.DataAccess.csproj", "SystemConfigurationService.DataAccess/"]
COPY ["SystemConfigurationService.Infrastructure/SystemConfigurationService.Infrastructure.csproj", "SystemConfigurationService.Infrastructure/"]

RUN dotnet restore "SystemConfigurationService.API/SystemConfigurationService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["SystemConfigurationService.Common/AppConfigurations/Jwt/keys/", "SystemConfigurationService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/SystemConfigurationService.API"
RUN dotnet build "SystemConfigurationService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SystemConfigurationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/SystemConfigurationService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r system-configuration-service && useradd -r -g system-configuration-service system-configuration-service
RUN chown -R system-configuration-service:system-configuration-service /app
USER system-configuration-service

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8051/health || exit 1

# Expose port
EXPOSE 8051

# Entry point
ENTRYPOINT ["dotnet", "SystemConfigurationService.API.dll"]