# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["TransactionService.API/TransactionService.API.csproj", "TransactionService.API/"]
COPY ["TransactionService.BusinessLogic/TransactionService.BusinessLogic.csproj", "TransactionService.BusinessLogic/"]
COPY ["TransactionService.Common/TransactionService.Common.csproj", "TransactionService.Common/"]
COPY ["TransactionService.DataAccess/TransactionService.DataAccess.csproj", "TransactionService.DataAccess/"]
COPY ["TransactionService.Infrastructure/TransactionService.Infrastructure.csproj", "TransactionService.Infrastructure/"]

RUN dotnet restore "TransactionService.API/TransactionService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["TransactionService.Common/AppConfigurations/Jwt/keys/", "TransactionService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/TransactionService.API"
RUN dotnet build "TransactionService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TransactionService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/TransactionService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r transaction-service-user && useradd -r -g transaction-service-user transaction-service-user
RUN chown -R transaction-service-user:transaction-service-user /app
USER transaction-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8076/health || exit 1

# Expose port
EXPOSE 8076

# Entry point
ENTRYPOINT ["dotnet", "TransactionService.API.dll"]