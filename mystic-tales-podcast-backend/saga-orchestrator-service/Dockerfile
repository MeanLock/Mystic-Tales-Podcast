# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["SagaOrchestratorService.API/SagaOrchestratorService.API.csproj", "SagaOrchestratorService.API/"]
COPY ["SagaOrchestratorService.BusinessLogic/SagaOrchestratorService.BusinessLogic.csproj", "SagaOrchestratorService.BusinessLogic/"]
COPY ["SagaOrchestratorService.Common/SagaOrchestratorService.Common.csproj", "SagaOrchestratorService.Common/"]
COPY ["SagaOrchestratorService.DataAccess/SagaOrchestratorService.DataAccess.csproj", "SagaOrchestratorService.DataAccess/"]
COPY ["SagaOrchestratorService.Infrastructure/SagaOrchestratorService.Infrastructure.csproj", "SagaOrchestratorService.Infrastructure/"]

RUN dotnet restore "SagaOrchestratorService.API/SagaOrchestratorService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["SagaOrchestratorService.Common/AppConfigurations/Jwt/keys/", "SagaOrchestratorService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/SagaOrchestratorService.API"
RUN dotnet build "SagaOrchestratorService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SagaOrchestratorService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/SagaOrchestratorService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r saga-orchestrator-service-user && useradd -r -g saga-orchestrator-service-user saga-orchestrator-service-user
RUN chown -R saga-orchestrator-service-user:saga-orchestrator-service-user /app
USER saga-orchestrator-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8081/health || exit 1

# Expose port
EXPOSE 8081

# Entry point
ENTRYPOINT ["dotnet", "SagaOrchestratorService.API.dll"]