# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["BookingManagementService.API/BookingManagementService.API.csproj", "BookingManagementService.API/"]
COPY ["BookingManagementService.BusinessLogic/BookingManagementService.BusinessLogic.csproj", "BookingManagementService.BusinessLogic/"]
COPY ["BookingManagementService.Common/BookingManagementService.Common.csproj", "BookingManagementService.Common/"]
COPY ["BookingManagementService.DataAccess/BookingManagementService.DataAccess.csproj", "BookingManagementService.DataAccess/"]
COPY ["BookingManagementService.Infrastructure/BookingManagementService.Infrastructure.csproj", "BookingManagementService.Infrastructure/"]

RUN dotnet restore "BookingManagementService.API/BookingManagementService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["BookingManagementService.Common/AppConfigurations/Jwt/keys/", "BookingManagementService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/BookingManagementService.API"
RUN dotnet build "BookingManagementService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BookingManagementService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/BookingManagementService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r booking-management-service-user && useradd -r -g booking-management-service-user booking-management-service-user
RUN chown -R booking-management-service-user:booking-management-service-user /app
USER booking-management-service-user

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8056/health || exit 1

# Expose port
EXPOSE 8056

# Entry point
ENTRYPOINT ["dotnet", "BookingManagementService.API.dll"]