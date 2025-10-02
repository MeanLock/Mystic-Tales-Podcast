# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["UserService.API/UserService.API.csproj", "UserService.API/"]
COPY ["UserService.BusinessLogic/UserService.BusinessLogic.csproj", "UserService.BusinessLogic/"]
COPY ["UserService.Common/UserService.Common.csproj", "UserService.Common/"]
COPY ["UserService.DataAccess/UserService.DataAccess.csproj", "UserService.DataAccess/"]
COPY ["UserService.Infrastructure/UserService.Infrastructure.csproj", "UserService.Infrastructure/"]

RUN dotnet restore "UserService.API/UserService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["UserService.Common/AppConfigurations/Jwt/keys/", "UserService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/UserService.API"
RUN dotnet build "UserService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/UserService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r user-service-user && useradd -r -g user-service-user user-service-user
RUN chown -R user-service-user:user-service-user /app
USER user-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8046/health || exit 1

# Expose port
EXPOSE 8046

# Entry point
ENTRYPOINT ["dotnet", "UserService.API.dll"]