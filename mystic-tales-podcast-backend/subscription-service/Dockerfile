# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["SubscriptionService.API/SubscriptionService.API.csproj", "SubscriptionService.API/"]
COPY ["SubscriptionService.BusinessLogic/SubscriptionService.BusinessLogic.csproj", "SubscriptionService.BusinessLogic/"]
COPY ["SubscriptionService.Common/SubscriptionService.Common.csproj", "SubscriptionService.Common/"]
COPY ["SubscriptionService.DataAccess/SubscriptionService.DataAccess.csproj", "SubscriptionService.DataAccess/"]
COPY ["SubscriptionService.Infrastructure/SubscriptionService.Infrastructure.csproj", "SubscriptionService.Infrastructure/"]

RUN dotnet restore "SubscriptionService.API/SubscriptionService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["SubscriptionService.Common/AppConfigurations/Jwt/keys/", "SubscriptionService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/SubscriptionService.API"
RUN dotnet build "SubscriptionService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SubscriptionService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/SubscriptionService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r subscription-service-user && useradd -r -g subscription-service-user subscription-service-user
RUN chown -R subscription-service-user:subscription-service-user /app
USER subscription-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8066/health || exit 1

# Expose port
EXPOSE 8066

# Entry point
ENTRYPOINT ["dotnet", "SubscriptionService.API.dll"]