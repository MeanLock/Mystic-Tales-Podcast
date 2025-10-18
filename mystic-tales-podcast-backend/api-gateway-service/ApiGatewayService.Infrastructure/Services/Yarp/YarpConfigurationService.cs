using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection; // Thêm cho GetService<T>()
using Yarp.ReverseProxy.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;
using Newtonsoft.Json;
using ApiGatewayService.Infrastructure.Services.Redis;

namespace ApiGatewayService.Infrastructure.Services.Yarp
{
    /// <summary>
    /// Service chịu trách nhiệm xây dựng và quản lý cấu hình YARP từ Consul và Redis.
    /// Tạo ra Routes và Clusters configuration từ service discovery data.
    /// </summary>
    /// <remarks>
    /// Service này đóng vai trò trung gian giữa Consul service discovery và YARP runtime configuration.
    /// Workflow: Consul Services → YARP Routes/Clusters → Runtime Configuration Update
    /// </remarks>
    public class YarpConfigurationService
    {
        #region Private Fields

        /// <summary>Cấu hình chính của YARP reverse proxy (load balancing, health check, caching)</summary>
        private readonly IYarpReverseProxyConfig _yarpReverseProxyConfig;
        
        /// <summary>Cấu hình routing của YARP (route patterns, transformations)</summary>
        private readonly IYarpRoutingConfig _yarpRoutingConfig;
        
        /// <summary>Cấu hình Consul service discovery connection</summary>
        private readonly IConsulServiceConfig _consulServiceConfig;
        
        /// <summary>Cấu hình Redis cache cho việc lưu trữ YARP configuration</summary>
        private readonly IRedisCacheConfig _redisCacheConfig;

        /// <summary>Service discovery service để lấy danh sách services từ Consul</summary>
        private readonly YarpServiceDiscoveryService _yarpServiceDiscoveryService;
        
        /// <summary>Redis cache service để lưu trữ configuration cho performance</summary>
        private readonly RedisCacheService _redisCacheService;
        
        /// <summary>Service provider để resolve InMemoryConfigProvider cho runtime updates</summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Logger để ghi log các hoạt động của service</summary>
        private readonly ILogger<YarpConfigurationService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo YarpConfigurationService với các dependencies cần thiết.
        /// </summary>
        /// <param name="yarpReverseProxyConfig">Cấu hình chính YARP (load balancing, health check)</param>
        /// <param name="yarpRoutingConfig">Cấu hình routing patterns và transformations</param>
        /// <param name="consulServiceConfig">Cấu hình kết nối Consul</param>
        /// <param name="redisCacheConfig">Cấu hình Redis cache</param>
        /// <param name="yarpServiceDiscoveryService">Service discovery từ Consul</param>
        /// <param name="redisCacheService">Redis cache service</param>
        /// <param name="serviceProvider">Service provider để resolve runtime services</param>
        /// <param name="logger">Logger service</param>
        public YarpConfigurationService(
            IYarpReverseProxyConfig yarpReverseProxyConfig,
            IYarpRoutingConfig yarpRoutingConfig,
            IConsulServiceConfig consulServiceConfig,
            IRedisCacheConfig redisCacheConfig,
            YarpServiceDiscoveryService yarpServiceDiscoveryService,
            RedisCacheService redisCacheService,
            IServiceProvider serviceProvider, // Inject service provider
            ILogger<YarpConfigurationService> logger)
        {
            _yarpReverseProxyConfig = yarpReverseProxyConfig;
            _yarpRoutingConfig = yarpRoutingConfig;
            _consulServiceConfig = consulServiceConfig;
            _redisCacheConfig = redisCacheConfig;
            _yarpServiceDiscoveryService = yarpServiceDiscoveryService;
            _redisCacheService = redisCacheService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Xây dựng cấu hình YARP hoàn chỉnh (Routes + Clusters) từ Consul service discovery.
        /// </summary>
        /// <returns>
        /// Tuple chứa:
        /// - Routes: Danh sách các route configurations cho YARP
        /// - Clusters: Danh sách các cluster configurations với destinations
        /// </returns>
        /// <exception cref="Exception">Ném khi có lỗi trong quá trình build configuration</exception>
        /// <remarks>
        /// Method này là entry point chính để tạo YARP configuration.
        /// Workflow: Consul Discovery → Build Routes → Build Clusters → Return Configuration
        /// </remarks>
        public async Task<(IReadOnlyList<RouteConfig> Routes, IReadOnlyList<ClusterConfig> Clusters)> GetProxyConfigAsync()
        {
            try
            {
                _logger.LogInformation("[YARP Building Proxy Configuration] YARP proxy configuration from Consul and Redis");

                var routes = await BuildRoutesFromConsulAsync();
                var clusters = await BuildClustersFromConsulAsync();

                _logger.LogInformation("[YARP Built Proxy Configuration]");
                return (routes, clusters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Error building YARP proxy configuration");
                throw;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Xây dựng danh sách Routes từ Consul service discovery.
        /// Mỗi service discovered sẽ tạo ra một route với pattern /api/{serviceName}/{**catch-all}.
        /// </summary>
        /// <returns>Danh sách readonly các RouteConfig</returns>
        /// <remarks>
        /// Route pattern: /api/{serviceName}/{**catch-all}
        /// Transform: Remove /api/{serviceName} prefix trước khi forward đến backend
        /// </remarks>
        private async Task<IReadOnlyList<RouteConfig>> BuildRoutesFromConsulAsync()
        {
            _logger.LogInformation("[YARP Building Routes] Building routes from Consul service discovery");
            var routes = new List<RouteConfig>();

            var serviceEndpoints = await _yarpServiceDiscoveryService.GetServiceEndpointsAsync();

            foreach (var serviceEntry in serviceEndpoints)
            {
                var serviceName = serviceEntry.Key;
                var endpoints = serviceEntry.Value;

                // Skip services không có healthy endpoints
                if (!endpoints.Any()) continue;

                // Tạo route config cho service
                // Route ID format: {serviceName}-route
                // Cluster ID format: {serviceName}-cluster (phải match với cluster config)
                var route = new RouteConfig
                {
                    RouteId = $"{serviceName}-route",
                    ClusterId = $"{serviceName}-cluster",
                    Match = new RouteMatch
                    {
                        // Route pattern: /api/{serviceName}/{**catch-all}
                        // Sẽ match tất cả requests đến /api/{serviceName}/...
                        Path = $"{_yarpRoutingConfig.DefaultRoutePrefix}/{serviceName}/{{**catch-all}}"
                    },
                    Transforms = new[]
                    {
                        new Dictionary<string, string>
                        {
                            // Transform: Remove /api/{serviceName} prefix, chỉ giữ lại {**catch-all}
                            // VD: /api/onlinehelpdeskservice/api/facilities → /api/facilities
                            { "PathPattern", "/{**catch-all}" }
                        }
                    }
                };

                _logger.LogInformation("==> [Created] route: {RouteId} with path pattern: {Path}",
                    route.RouteId, route.Match.Path);

                routes.Add(route);
            }

            _logger.LogInformation("[YARP Built Routes] Built {RouteCount} routes from Consul service discovery", routes.Count);

            return routes.AsReadOnly();
        }

        /// <summary>
        /// Xây dựng danh sách Clusters từ Consul service discovery.
        /// Mỗi cluster chứa các destination endpoints và cấu hình load balancing, health check.
        /// </summary>
        /// <returns>Danh sách readonly các ClusterConfig</returns>
        /// <remarks>
        /// Cluster bao gồm:
        /// - Destinations: Danh sách các endpoint instances của service
        /// - Load Balancing Policy: Thuật toán phân tải traffic
        /// - Health Check: Cấu hình kiểm tra sức khỏe endpoints
        /// </remarks>
        private async Task<IReadOnlyList<ClusterConfig>> BuildClustersFromConsulAsync()
        {
            _logger.LogInformation("[YARP Building Clusters] Building clusters from Consul service discovery");
            var clusters = new List<ClusterConfig>();

            var serviceEndpoints = await _yarpServiceDiscoveryService.GetServiceEndpointsAsync();

            foreach (var serviceEntry in serviceEndpoints)
            {
                var serviceName = serviceEntry.Key;
                var endpoints = serviceEntry.Value;

                // Skip services không có healthy endpoints
                if (!endpoints.Any()) continue;

                // Tạo destinations map từ service endpoints với configured pattern từ Consul
                // Pattern: {serviceName}-{environment}-{instanceNumber:D3} (configured in Consul section)
                // Example: user-api-service-dev-001, user-api-service-dev-002
                var destinations = new Dictionary<string, DestinationConfig>();
                for (int i = 0; i < endpoints.Count; i++)
                {
                    var instanceNumber = i + 1;
                    var destinationId = GenerateDestinationId(serviceName, instanceNumber);
                    
                    destinations[destinationId] = new DestinationConfig
                    {
                        Address = endpoints[i]
                    };
                }

                // Tạo cluster config với load balancing và health check
                var cluster = new ClusterConfig
                {
                    ClusterId = $"{serviceName}-cluster", // Phải match với RouteConfig.ClusterId
                    LoadBalancingPolicy = _yarpReverseProxyConfig.LoadBalancing.DefaultPolicy, // RoundRobin, Random, etc.
                    Destinations = destinations,
                    HealthCheck = new HealthCheckConfig
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = _yarpReverseProxyConfig.HealthCheck.EnableHealthChecks,
                            Interval = TimeSpan.FromSeconds(_yarpReverseProxyConfig.HealthCheck.HealthCheckIntervalSeconds),
                            Path = _yarpReverseProxyConfig.HealthCheck.HealthCheckPath, // Thường là "/health"
                            Timeout = TimeSpan.FromSeconds(_yarpReverseProxyConfig.HealthCheck.HealthCheckTimeoutSeconds)
                        }
                    }
                };

                _logger.LogInformation("==> [Created] cluster: {ClusterId} with {DestinationCount} destinations: {Destinations}",
                    cluster.ClusterId, destinations.Count, string.Join(", ", destinations.Select(d => $"{d.Key}={d.Value.Address}")));

                clusters.Add(cluster);
            }
            _logger.LogInformation("[YARP Built Clusters] Built {ClusterCount} clusters from Consul service discovery", clusters.Count);

            return clusters.AsReadOnly();
        }

        /// <summary>
        /// Generate destination instance ID based on configured pattern.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="instanceNumber">Instance number (1-based)</param>
        /// <returns>Formatted destination ID</returns>
        /// <example>
        /// GenerateDestinationId("user-api-service", 1) → "user-api-service-dev-001"
        /// GenerateDestinationId("order-processing", 5) → "order-processing-dev-005"
        /// </example>
        private string GenerateDestinationId(string serviceName, int instanceNumber)
        {
            return _consulServiceConfig.DestinationInstancePattern
                .Replace("{serviceName}", serviceName)
                .Replace("{environment}", _consulServiceConfig.DestinationEnvironment)
                .Replace("{instanceNumber:D3}", instanceNumber.ToString("D3"));
        }

        #endregion
    }
}