using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SagaOrchestratorService.Common.AppConfigurations.App.interfaces;
using SagaOrchestratorService.Common.Configurations.Consul.interfaces;

namespace SagaOrchestratorService.API.Configurations.App
{
    public static class HealthCheckConfig
    {
        public static void UseAppHealthCheckConfig(this WebApplication app)
        {
            // Lấy configuration từ DI container
            var appConfig = app.Services.GetRequiredService<IAppConfig>();
            var consulServiceConfig = app.Services.GetRequiredService<IConsulServiceConfig>();
            
            // Sử dụng endpoint từ config thay vì hard code
            var healthCheckEndpoint = appConfig.HEALTH_CHECK_ENDPOINT;
            
            // Endpoint cho health checks
            app.MapHealthChecks(healthCheckEndpoint, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
               ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        status = report.Status.ToString().ToLower(),
                        timestamp = DateTime.UtcNow,
                        service = consulServiceConfig.ServiceName ?? "SurveyTalk_Service", // Lấy từ consul config
                        version = "1.0.0", // TODO: Có thể lấy từ appsettings
                        serviceId = consulServiceConfig.Id, // Lấy service ID từ consul config
                        totalDuration = report.TotalDuration.TotalMilliseconds,
                        checks = report.Entries.ToDictionary(
                            entry => entry.Key,
                            entry => new
                            {
                                status = entry.Value.Status.ToString().ToLower(),
                                description = entry.Value.Description,
                                duration = entry.Value.Duration.TotalMilliseconds,
                                exception = entry.Value.Exception?.Message
                            }
                        )
                    };

                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(jsonResponse);
                }
            });
        }
    }
}
