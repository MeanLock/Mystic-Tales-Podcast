namespace ApiGatewayService.Infrastructure.Configurations.Consul.interfaces
{
    public interface IConsulServiceConfig
    {
        string Host { get; set; }
        string Id { get; set; }
        string ServiceName { get; set; }
        string ServiceScheme { get; set; }

        List<string> Tags { get; set; }
        string Version { get; set; }  // Added for service versioning
        
        string DestinationInstancePattern { get; set; }
        string DestinationEnvironment { get; set; }
    }
}
