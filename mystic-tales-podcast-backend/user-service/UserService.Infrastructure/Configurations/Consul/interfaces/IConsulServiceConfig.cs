namespace UserService.Infrastructure.Configurations.Consul.interfaces
{
    public interface IConsulServiceConfig
    {
        string Host { get; set; }
        string Id { get; set; }
        string ServiceName { get; set; }
        string ServiceScheme { get; set; }
        List<string> Tags { get; set; }
    }
}
