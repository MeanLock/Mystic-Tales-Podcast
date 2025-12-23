# # Build stage
# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /src

# # Copy csproj files và restore dependencies
# COPY ["PodcastService.API/PodcastService.API.csproj", "PodcastService.API/"]
# COPY ["PodcastService.BusinessLogic/PodcastService.BusinessLogic.csproj", "PodcastService.BusinessLogic/"]
# COPY ["PodcastService.Common/PodcastService.Common.csproj", "PodcastService.Common/"]
# COPY ["PodcastService.DataAccess/PodcastService.DataAccess.csproj", "PodcastService.DataAccess/"]
# COPY ["PodcastService.Infrastructure/PodcastService.Infrastructure.csproj", "PodcastService.Infrastructure/"]

# RUN dotnet restore "PodcastService.API/PodcastService.API.csproj"

# # Copy source code và build
# COPY . .

# # Ensure JWT keys are copied
# COPY ["PodcastService.Common/AppConfigurations/Jwt/keys/", "PodcastService.Common/AppConfigurations/Jwt/keys/"]

# WORKDIR "/src/PodcastService.API"
# RUN dotnet build "PodcastService.API.csproj" -c Release -o /app/build

# # Publish stage
# FROM build AS publish
# RUN dotnet publish "PodcastService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# # Runtime stage
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# WORKDIR /app

# # Install curl cho health checks
# RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# # Copy published application
# COPY --from=publish /app/publish .

# # Copy JWT keys explicitly to ensure they exist
# COPY --from=build /src/PodcastService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# # Create non-root user
# RUN groupadd -r podcast-service-user && useradd -r -g podcast-service-user podcast-service-user
# RUN chown -R podcast-service-user:podcast-service-user /app
# USER podcast-service-user

# # Health check
# HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
#     CMD curl -f http://localhost:8061/health || exit 1

# # Expose port
# EXPOSE 8061

# # Entry point
# ENTRYPOINT ["dotnet", "PodcastService.API.dll"]


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["PodcastService.API/PodcastService.API.csproj", "PodcastService.API/"]
COPY ["PodcastService.BusinessLogic/PodcastService.BusinessLogic.csproj", "PodcastService.BusinessLogic/"]
COPY ["PodcastService.Common/PodcastService.Common.csproj", "PodcastService.Common/"]
COPY ["PodcastService.DataAccess/PodcastService.DataAccess.csproj", "PodcastService.DataAccess/"]
COPY ["PodcastService.Infrastructure/PodcastService.Infrastructure.csproj", "PodcastService.Infrastructure/"]

RUN dotnet restore "PodcastService.API/PodcastService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["PodcastService.Common/AppConfigurations/Jwt/keys/", "PodcastService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/PodcastService.API"
RUN dotnet build "PodcastService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PodcastService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install dependencies (curl + ffmpeg) - MUST be before USER command
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        curl \
        ffmpeg \
    && rm -rf /var/lib/apt/lists/* \
    || (sleep 5 && apt-get update && apt-get install -y --fix-missing curl ffmpeg)

# RUN apt-get update && \
#     apt-get install -y \
#         curl \
#         ffmpeg \
#     && rm -rf /var/lib/apt/lists/*

# Verify FFmpeg installation (optional, helps catch issues early)
RUN ffmpeg -version && ffprobe -version

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/PodcastService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r podcast-service-user && useradd -r -g podcast-service-user podcast-service-user
RUN chown -R podcast-service-user:podcast-service-user /app
USER podcast-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8061/health || exit 1

# Expose port
EXPOSE 8061

# Entry point
ENTRYPOINT ["dotnet", "PodcastService.API.dll"]