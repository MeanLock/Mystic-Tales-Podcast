# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files và restore dependencies
COPY ["SurveyTalkService.API/SurveyTalkService.API.csproj", "SurveyTalkService.API/"]
COPY ["SurveyTalkService.BusinessLogic/SurveyTalkService.BusinessLogic.csproj", "SurveyTalkService.BusinessLogic/"]
COPY ["SurveyTalkService.Common/SurveyTalkService.Common.csproj", "SurveyTalkService.Common/"]
COPY ["SurveyTalkService.DataAccess/SurveyTalkService.DataAccess.csproj", "SurveyTalkService.DataAccess/"]
COPY ["SurveyTalkService.Infrastructure/SurveyTalkService.Infrastructure.csproj", "SurveyTalkService.Infrastructure/"]

RUN dotnet restore "SurveyTalkService.API/SurveyTalkService.API.csproj"

# Copy source code và build
COPY . .

# Ensure JWT keys are copied
COPY ["SurveyTalkService.Common/AppConfigurations/Jwt/keys/", "SurveyTalkService.Common/AppConfigurations/Jwt/keys/"]

WORKDIR "/src/SurveyTalkService.API"
RUN dotnet build "SurveyTalkService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SurveyTalkService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Copy JWT keys explicitly to ensure they exist
COPY --from=build /src/SurveyTalkService.Common/AppConfigurations/Jwt/keys/ ./AppConfigurations/Jwt/keys/

# Create non-root user
RUN groupadd -r surveytalk && useradd -r -g surveytalk surveytalk
RUN chown -R surveytalk:surveytalk /app
USER surveytalk

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8021/health || exit 1

# Expose port
EXPOSE 8021

# Entry point
ENTRYPOINT ["dotnet", "SurveyTalkService.API.dll"]