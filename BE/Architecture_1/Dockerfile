# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["Architecture_1.API/Architecture_1.API.csproj", "Architecture_1.API/"]
COPY ["Architecture_1.BusinessLogic/Architecture_1.BusinessLogic.csproj", "Architecture_1.BusinessLogic/"]
COPY ["Architecture_1.Common/Architecture_1.Common.csproj", "Architecture_1.Common/"]
COPY ["Architecture_1.DataAccess/Architecture_1.DataAccess.csproj", "Architecture_1.DataAccess/"]
COPY ["Architecture_1.Infrastructure/Architecture_1.Infrastructure.csproj", "Architecture_1.Infrastructure/"]

RUN dotnet restore "Architecture_1.API/Architecture_1.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["Architecture_1.Common/AppConfigurations/Jwt/keys/", "Architecture_1.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/Architecture_1.API"
RUN dotnet build "Architecture_1.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Architecture_1.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/Architecture_1.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r helpdesk && useradd -r -g helpdesk helpdesk
RUN chown -R helpdesk:helpdesk /app
USER helpdesk

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8016/health || exit 1

# Expose port
EXPOSE 8016

# Entry point
ENTRYPOINT ["dotnet", "Architecture_1.API.dll"]