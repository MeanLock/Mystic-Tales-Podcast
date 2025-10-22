# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["ModerationService.API/ModerationService.API.csproj", "ModerationService.API/"]
COPY ["ModerationService.BusinessLogic/ModerationService.BusinessLogic.csproj", "ModerationService.BusinessLogic/"]
COPY ["ModerationService.Common/ModerationService.Common.csproj", "ModerationService.Common/"]
COPY ["ModerationService.DataAccess/ModerationService.DataAccess.csproj", "ModerationService.DataAccess/"]
COPY ["ModerationService.Infrastructure/ModerationService.Infrastructure.csproj", "ModerationService.Infrastructure/"]

RUN dotnet restore "ModerationService.API/ModerationService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["ModerationService.Common/AppConfigurations/Jwt/keys/", "ModerationService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/ModerationService.API"
RUN dotnet build "ModerationService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ModerationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/ModerationService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r moderation-service-user && useradd -r -g moderation-service-user moderation-service-user
RUN chown -R moderation-service-user:moderation-service-user /app
USER moderation-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8071/health || exit 1

# Expose port
EXPOSE 8071

# Entry point
ENTRYPOINT ["dotnet", "ModerationService.API.dll"]