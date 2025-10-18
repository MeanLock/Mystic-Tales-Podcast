using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Yarp
{
    public class YarpRoutingConfigModel
    {
        public bool EnableDynamicRouting { get; set; } = true;
        public int RouteRefreshIntervalSeconds { get; set; } = 60;
        public string DefaultRoutePrefix { get; set; } = "/api";
        public bool EnableRouteMetrics { get; set; } = true;
        public bool EnableRouteLogging { get; set; } = true;
        public string[] ExcludedRoutePatterns { get; set; } = new[] { "/health", "/metrics", "/swagger" };
        public string[] EligibleServiceTags { get; set; } = new[] { "api", "microservice" };
        public string[] ExcludedServiceNames { get; set; } = new[] { "consul", "api-gateway" };
        public string DefaultServiceScheme { get; set; } = "http";
        public int RefreshRetryDelaySeconds { get; set; } = 10;
        public int MaxRetryAttempts { get; set; } = 3;
    }

    public class YarpRoutingConfig : IYarpRoutingConfig
    {
        public bool EnableDynamicRouting { get; set; } = true;
        public int RouteRefreshIntervalSeconds { get; set; } = 60;
        public string DefaultRoutePrefix { get; set; } = "/api";
        public bool EnableRouteMetrics { get; set; } = true;
        public bool EnableRouteLogging { get; set; } = true;
        public string[] ExcludedRoutePatterns { get; set; } = new[] { "/health", "/metrics", "/swagger" };
        public string[] EligibleServiceTags { get; set; } = new[] { "api", "microservice" };
        public string[] ExcludedServiceNames { get; set; } = new[] { "consul", "api-gateway" };
        public string DefaultServiceScheme { get; set; } = "http";
        public int RefreshRetryDelaySeconds { get; set; } = 10;
        public int MaxRetryAttempts { get; set; } = 3;

        public YarpRoutingConfig(IConfiguration configuration)
        {
            var routingConfig = configuration.GetSection("Infrastructure:Yarp:Routing")
                .Get<YarpRoutingConfigModel>();
            if (routingConfig != null)
            {
                EnableDynamicRouting = routingConfig.EnableDynamicRouting;
                RouteRefreshIntervalSeconds = routingConfig.RouteRefreshIntervalSeconds;
                DefaultRoutePrefix = routingConfig.DefaultRoutePrefix;
                EnableRouteMetrics = routingConfig.EnableRouteMetrics;
                EnableRouteLogging = routingConfig.EnableRouteLogging;
                ExcludedRoutePatterns = routingConfig.ExcludedRoutePatterns;
                EligibleServiceTags = routingConfig.EligibleServiceTags;
                ExcludedServiceNames = routingConfig.ExcludedServiceNames;
                DefaultServiceScheme = routingConfig.DefaultServiceScheme;
                RefreshRetryDelaySeconds = routingConfig.RefreshRetryDelaySeconds;
                MaxRetryAttempts = routingConfig.MaxRetryAttempts;
            }
        }
    }
}