namespace TransactionService.Common.Configurations.Consul.interfaces
{
    public interface IConsulHealthCheckConfig
    {
        bool Enabled { get; set; }
        string Name { get; set; }
        string Notes { get; set; }
        string BaseUrl { get; set; }
        int Timeout { get; set; }
        int Interval { get; set; }
        int DeregisterCriticalServiceAfter { get; set; }
    }
}
