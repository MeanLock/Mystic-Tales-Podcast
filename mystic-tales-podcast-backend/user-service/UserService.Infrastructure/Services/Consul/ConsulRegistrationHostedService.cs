using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure.Configurations.Consul.interfaces;
using UserService.Common.AppConfigurations.App.interfaces;

namespace UserService.Infrastructure.Services.Consul
{
    public class ConsulRegistrationHostedService : IHostedService
    {
        private readonly IConsulClient _consulClient;
        private readonly IConsulServiceConfig _consulServiceConfig;
        private readonly IConsulHealthCheckConfig _consulHealthCheckConfig;
        private readonly IAppConfig _appConfig;
        private readonly ILogger<ConsulRegistrationHostedService> _logger;
        private string _registrationId = string.Empty;

        public ConsulRegistrationHostedService(
            IConsulClient consulClient,
            IConsulServiceConfig consulServiceConfig,
            IConsulHealthCheckConfig consulHealthCheckConfig,
            IAppConfig appConfig,
            ILogger<ConsulRegistrationHostedService> logger)
        {
            _consulClient = consulClient;
            _consulServiceConfig = consulServiceConfig;
            _consulHealthCheckConfig = consulHealthCheckConfig;
            _appConfig = appConfig;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("ConsulService.StartAsync() - BEGIN");

                // Lấy thông tin từ configuration
                var serviceId = _consulServiceConfig.Id;
                var serviceName = _consulServiceConfig.ServiceName;
                var serviceScheme = _consulServiceConfig.ServiceScheme;
                var serviceTags = _consulServiceConfig.Tags?.ToArray() ?? new string[] { };
                var serviceUri = new Uri(_appConfig.APP_BASE_URL); // Current running service


                _logger.LogInformation($"Service configuration - ID: {serviceId}, Name: {serviceName}, Address: {serviceUri.Host}, Port: {serviceUri.Port}");

                _registrationId = serviceId;

                // Tạo registration cho service
                var registration = new AgentServiceRegistration()
                {
                    ID = serviceId,
                    Name = serviceName,
                    Address = serviceUri.Host,
                    Port = serviceUri.Port,
                    Tags = serviceTags
                };

                // Thêm health check nếu được enable
                if (_consulHealthCheckConfig.Enabled)
                {
                    // Kết hợp BaseUrl từ consul config với HEALTH_CHECK_ENDPOINT từ app config
                    var healthCheckUrl = $"{_consulHealthCheckConfig.BaseUrl.TrimEnd('/')}{_appConfig.HEALTH_CHECK_ENDPOINT}";
                    
                    Console.WriteLine($"Health Check Enabled: {healthCheckUrl}");
                    
                    registration.Check = new AgentServiceCheck()
                    {
                        HTTP = healthCheckUrl,
                        Timeout = TimeSpan.FromSeconds(_consulHealthCheckConfig.Timeout),
                        Interval = TimeSpan.FromSeconds(_consulHealthCheckConfig.Interval),
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(_consulHealthCheckConfig.DeregisterCriticalServiceAfter),
                        Name = _consulHealthCheckConfig.Name,
                        Notes = _consulHealthCheckConfig.Notes
                    };
                }
                else
                {
                    Console.WriteLine("Health Check Disabled");
                }

                // Đăng ký service với Consul
                await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
                
                _logger.LogInformation($"Service {serviceName} registered with Consul successfully. ID: {serviceId}");
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
                    _logger.LogInformation($"Service {_registrationId} deregistered from Consul successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deregister service from Consul");
            }
        }

        private static string ExtractHostFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "localhost";
            }
        }
    }
}
