# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["ApiGatewayService.Entry/ApiGatewayService.Entry.csproj", "ApiGatewayService.Entry/"]
COPY ["ApiGatewayService.Application/ApiGatewayService.Application.csproj", "ApiGatewayService.Application/"]
COPY ["ApiGatewayService.Infrastructure/ApiGatewayService.Infrastructure.csproj", "ApiGatewayService.Infrastructure/"]
COPY ["ApiGatewayService.Common/ApiGatewayService.Common.csproj", "ApiGatewayService.Common/"]

RUN dotnet restore "ApiGatewayService.Entry/ApiGatewayService.Entry.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["ApiGatewayService.Common/AppConfigurations/Jwt/keys/", "ApiGatewayService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/ApiGatewayService.Entry"
RUN dotnet build "ApiGatewayService.Entry.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ApiGatewayService.Entry.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/ApiGatewayService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r api-gateway-service-user && useradd -r -g api-gateway-service-user api-gateway-service-user
RUN chown -R api-gateway-service-user:api-gateway-service-user /app
USER api-gateway-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8041/health || exit 1

# Expose port
EXPOSE 8041

# Entry point
ENTRYPOINT ["dotnet", "ApiGatewayService.Entry.dll"]
