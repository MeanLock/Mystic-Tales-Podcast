namespace ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces
{
    public interface IYarpRoutingConfig
    {
        bool EnableDynamicRouting { get; set; }
        int RouteRefreshIntervalSeconds { get; set; }
        string DefaultRoutePrefix { get; set; }
        bool EnableRouteMetrics { get; set; }
        bool EnableRouteLogging { get; set; }
        string[] ExcludedRoutePatterns { get; set; }
        string[] EligibleServiceTags { get; set; }
        string[] ExcludedServiceNames { get; set; }
        string DefaultServiceScheme { get; set; }
        int RefreshRetryDelaySeconds { get; set; }
        int MaxRetryAttempts { get; set; }
    }
}
