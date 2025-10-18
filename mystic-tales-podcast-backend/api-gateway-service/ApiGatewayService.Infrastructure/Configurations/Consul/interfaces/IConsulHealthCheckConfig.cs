namespace ApiGatewayService.Infrastructure.Configurations.Consul.interfaces
{
    public interface IConsulHealthCheckConfig
    {
        bool Enabled { get; set; }
        string Name { get; set; }
        string Notes { get; set; }
        string BaseUrl { get; set; }  // Changed from Host to BaseUrl - more semantic
        int IntervalSeconds { get; set; }
        int TimeoutSeconds { get; set; }
        int DeregisterCriticalServiceAfter { get; set; }
    }
}
