using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Consul;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;
using ApiGatewayService.Common.AppConfigurations.App.interfaces;

namespace ApiGatewayService.Infrastructure.Services.Consul
{
    public class ConsulRegistrationHostedService : IHostedService
    {
        private readonly IAppConfig _appConfig;
        private readonly IConsulClient _consulClient;
        private readonly IConsulServiceConfig _consulServiceConfig;
        private readonly IConsulHealthCheckConfig _consulHealthCheckConfig;
        private readonly ILogger<ConsulRegistrationHostedService> _logger;
        private string? _registrationId;

        public ConsulRegistrationHostedService(
            IAppConfig appConfig,
            IConsulClient consulClient,
            IConsulServiceConfig consulServiceConfig,
            IConsulHealthCheckConfig consulHealthCheckConfig,
            ILogger<ConsulRegistrationHostedService> logger)
        {
            _appConfig = appConfig;
            _consulClient = consulClient;
            _consulServiceConfig = consulServiceConfig;
            _consulHealthCheckConfig = consulHealthCheckConfig;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _registrationId = $"{_consulServiceConfig.Id}";

                // Parse service host URL to extract host and port for local service registration
                var serviceUri = new Uri(_appConfig.APP_BASE_URL); // Current running service
                
                var registration = new AgentServiceRegistration
                {
                    ID = _registrationId,
                    Name = _consulServiceConfig.ServiceName,
                    Address = serviceUri.Host,
                    Port = serviceUri.Port,
                    Tags = _consulServiceConfig.Tags?.ToArray(),
                };

                if (_consulHealthCheckConfig.Enabled)
                {
                    // Use BaseUrl from config, fallback to APP_BASE_URL if BaseUrl is empty
                    var baseUrl = !string.IsNullOrEmpty(_consulHealthCheckConfig.BaseUrl) 
                        ? _consulHealthCheckConfig.BaseUrl 
                        : _appConfig.APP_BASE_URL;
                        
                    // Build health check URL from BaseUrl + HEALTH_CHECK_ENDPOINT from app config
                    var healthCheckUrl = $"{baseUrl}{_appConfig.HEALTH_CHECK_ENDPOINT}";
                    Console.WriteLine($"Health Check Enabled: {healthCheckUrl}");
                    
                    registration.Check = new AgentServiceCheck
                    {
                        HTTP = healthCheckUrl,
                        Interval = TimeSpan.FromSeconds(_consulHealthCheckConfig.IntervalSeconds),
                        Timeout = TimeSpan.FromSeconds(_consulHealthCheckConfig.TimeoutSeconds),
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(_consulHealthCheckConfig.DeregisterCriticalServiceAfter)
                    };
                }
                else
                {
                    registration.Check = null; // Disable health check if not enabled
                }

                await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
                _logger.LogInformation("Service {ServiceName} with ID {ServiceId} registered successfully with Consul", 
                    _consulServiceConfig.ServiceName, _registrationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register service with Consul");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(_registrationId))
                {
                    await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
                    _logger.LogInformation("Service {ServiceId} deregistered successfully from Consul", _registrationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deregister service from Consul");
            }
        }
    }
}
