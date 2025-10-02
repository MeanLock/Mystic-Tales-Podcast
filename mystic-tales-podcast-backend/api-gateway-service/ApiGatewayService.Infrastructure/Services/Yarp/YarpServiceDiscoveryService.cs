using Microsoft.Extensions.Logging;
using Consul;
using ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;
using ApiGatewayService.Infrastructure.Services.Redis;

namespace ApiGatewayService.Infrastructure.Services.Yarp
{
    /// <summary>
    /// Service chịu trách nhiệm discovery và quản lý thông tin các backend services từ Consul.
    /// Cung cấp danh sách services eligible cho YARP routing và healthy endpoints.
    /// </summary>
    /// <remarks>
    /// Service này tương tác trực tiếp với Consul để:
    /// - Lấy danh sách các services đang hoạt động
    /// - Filter services theo rules (excluded services, required tags)
    /// - Kiểm tra health status của service instances
    /// - Cung cấp endpoint information cho YARP configuration
    /// </remarks>
    public class YarpServiceDiscoveryService
    {
        #region Private Fields

        /// <summary>Consul client để tương tác với Consul service discovery</summary>
        private readonly IConsulClient _consulClient;

        /// <summary>Cấu hình routing rules (eligible tags, excluded services)</summary>
        private readonly IYarpRoutingConfig _yarpRoutingConfig;

        /// <summary>Cấu hình Consul connection và service registration</summary>
        private readonly IConsulServiceConfig _consulServiceConfig;

        /// <summary>Cấu hình Redis cache keys và TTL settings</summary>
        private readonly IRedisCacheConfig _redisCacheConfig;

        /// <summary>Service để quản lý Redis cache operations</summary>
        private readonly RedisCacheService _redisCacheService;

        /// <summary>Logger để ghi log các hoạt động discovery</summary>
        private readonly ILogger<YarpServiceDiscoveryService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo YarpServiceDiscoveryService với các dependencies cần thiết.
        /// </summary>
        /// <param name="consulClient">Consul client để query services</param>
        /// <param name="yarpRoutingConfig">Cấu hình routing rules</param>
        /// <param name="consulServiceConfig">Cấu hình Consul connection</param>
        /// <param name="redisCacheConfig">Cấu hình Redis cache keys và TTL</param>
        /// <param name="logger">Logger service</param>
        public YarpServiceDiscoveryService(
            IConsulClient consulClient,
            IYarpRoutingConfig yarpRoutingConfig,
            IConsulServiceConfig consulServiceConfig,
            IRedisCacheConfig redisCacheConfig,

            RedisCacheService redisCacheService,

            ILogger<YarpServiceDiscoveryService> logger)
        {
            _consulClient = consulClient;
            _yarpRoutingConfig = yarpRoutingConfig;
            _consulServiceConfig = consulServiceConfig;
            _redisCacheConfig = redisCacheConfig;

            _redisCacheService = redisCacheService;

            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Lấy danh sách tất cả services available từ Consul, đã filter theo rules.
        /// </summary>
        /// <returns>Danh sách service names eligible cho YARP routing</returns>
        /// <exception cref="Exception">Ném khi có lỗi kết nối với Consul</exception>
        /// <remarks>
        /// Filter rules:
        /// - Loại bỏ excluded services (từ YarpRoutingConfig.ExcludedServiceNames)
        /// - Loại bỏ chính API Gateway service
        /// - Chỉ lấy services có required tags (từ YarpRoutingConfig.EligibleServiceTags)
        /// </remarks>
        public async Task<List<string>> GetAvailableServicesAsync()
        {
            try
            {
                _logger.LogInformation("[YARP Discovering Eligible Services] Discovering services from Consul for YARP routing");

                var servicesResponse = await _consulClient.Catalog.Services();
                if (servicesResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogWarning("[YARP Warn] Failed to retrieve services from Consul. Status: {StatusCode}", servicesResponse.StatusCode);
                    return new List<string>();
                }

                // Sử dụng config thay vì hard-code
                var eligibleServices = servicesResponse.Response
                    .Where(kvp =>
                        // Loại bỏ excluded services từ config
                        !_yarpRoutingConfig.ExcludedServiceNames.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase) &&
                        // Loại bỏ chính API Gateway
                        !kvp.Key.Equals(_consulServiceConfig.ServiceName, StringComparison.OrdinalIgnoreCase) &&
                        // Chỉ lấy services có tags eligible từ config
                        kvp.Value != null && _yarpRoutingConfig.EligibleServiceTags.Any(tag =>
                            kvp.Value.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    )
                    .Select(kvp => kvp.Key)
                    .ToList();


                foreach (var serviceName in eligibleServices)
                {
                    _logger.LogInformation("==> [Discovered] Eligible service found: {ServiceName}", serviceName);
                }
                _logger.LogInformation("[YARP Discovered Eligible Services] Discovered {ServiceCount} eligible services from Consul", eligibleServices.Count);


                return eligibleServices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Error discovering services from Consul");
                return new List<string>();
            }
        }

        /// <summary>
        /// Lấy danh sách healthy service instances cho một service cụ thể.
        /// Method này được sử dụng để debug và monitoring.
        /// </summary>
        /// <param name="serviceName">Tên service cần query</param>
        /// <returns>Danh sách ServiceEndpoint objects với thông tin chi tiết</returns>
        /// <exception cref="Exception">Ném khi có lỗi query Consul</exception>
        /// <remarks>
        /// ServiceEndpoint bao gồm: ServiceName, Address, Port, Tags
        /// Chỉ trả về instances có health status = Passing
        /// </remarks>
        public async Task<List<ServiceEndpoint>> GetHealthyServiceInstancesAsync(string serviceName)
        {
            try
            {
                _logger.LogInformation("[YARP Getting Healthy Instances] Getting healthy instances for service {ServiceName}", serviceName);
                // Query healthy services từ Consul (chỉ lấy Passing health status)
                var healthyServicesResponse = await _consulClient.Health.Service(serviceName, string.Empty, true);

                if (healthyServicesResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Convert Consul ServiceEntry thành ServiceEndpoint objects
                    var endpoints = healthyServicesResponse.Response.Select(entry => new ServiceEndpoint
                    {
                        ServiceName = entry.Service.Service,
                        Address = entry.Service.Address,
                        Port = entry.Service.Port,
                        Tags = entry.Service.Tags?.ToList() ?? new List<string>()
                    }).ToList();

                    _logger.LogInformation("==> [Found] {InstanceCount} healthy instances for service {ServiceName}",
                        endpoints.Count, serviceName);

                    _logger.LogInformation("[YARP Got Healthy Instances]");
                    return endpoints;
                }

                _logger.LogInformation("* No healthy instances found for service {ServiceName}", serviceName);
                return new List<ServiceEndpoint>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Error getting healthy instances for service {ServiceName}", serviceName);
                return new List<ServiceEndpoint>();
            }
        }

        /// <summary>
        /// Lấy service endpoints với caching để giảm tải Consul.
        /// Sử dụng Redis cache với TTL được cấu hình để tránh gọi Consul quá thường xuyên.
        /// </summary>
        /// <returns>Dictionary mapping service names đến danh sách endpoint URLs</returns>
        /// <remarks>
        /// Caching Strategy:
        /// - Cache TTL: từ RedisCacheConfig.ServiceEndpointsTtlSeconds (default 60s)
        /// - Cache key: từ RedisCacheConfig.ServiceEndpointsKey
        /// - Fallback: Fresh data từ Consul nếu cache miss
        /// - Performance: Giảm 75-90% calls đến Consul
        /// </remarks>
        public async Task<Dictionary<string, List<string>>> GetServiceEndpointsAsync()
        {
            // Sử dụng configured cache key thay vì hard-code
            var cacheKey = _redisCacheConfig.ServiceEndpointsKey;

            try
            {
                // Try cache first để giảm tải Consul
                var cached = await TryGetCachedEndpointsAsync(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("[YARP Cache Hit] Using cached service endpoints ({ServiceCount} services) with TTL {TtlSeconds}s",
                        cached.Count, _redisCacheConfig.ServiceEndpointsTtlSeconds);

                    return cached;
                }

                // Cache miss - get fresh data từ Consul
                _logger.LogInformation("[YARP Cache Miss] Building fresh service endpoints from Consul");
                var freshEndpoints = await GetFreshServiceEndpointsAsync();

                // Cache cho lần sau để improve performance với configured TTL
                await CacheServiceEndpointsAsync(cacheKey, freshEndpoints);

                return freshEndpoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Error getting service endpoints");

                // Fallback đến stale cache nếu có error với Consul
                var staleCache = await TryGetStaleEndpointsAsync(cacheKey);
                if (staleCache != null)
                {
                    _logger.LogWarning("[YARP Stale Cache] Using stale cached endpoints due to Consul error (TTL {StaleTtlSeconds}s)",
                        _redisCacheConfig.StaleCacheTtlSeconds);
                    return staleCache;
                }

                return new Dictionary<string, List<string>>();
            }
        }

        /// <summary>
        /// Get fresh service endpoints từ Consul (không cache).
        /// Method này luôn gọi Consul để lấy data mới nhất.
        /// </summary>
        /// <returns>Fresh dictionary mapping service names đến endpoint URLs</returns>
        private async Task<Dictionary<string, List<string>>> GetFreshServiceEndpointsAsync()
        {
            try
            {
                var serviceNames = await GetAvailableServicesAsync();
                var serviceEndpoints = new Dictionary<string, List<string>>();

                foreach (var serviceName in serviceNames)
                {
                    var instances = await GetHealthyServiceInstancesAsync(serviceName);
                    if (instances.Any())
                    {
                        // Sử dụng config scheme thay vì hard-code
                        serviceEndpoints[serviceName] = instances.Select(instance =>
                            $"{_yarpRoutingConfig.DefaultServiceScheme}://{instance.Address}:{instance.Port}").ToList();

                        // in mọi thông tin của service
                        _logger.LogInformation("==> [Service Info] Service '{ServiceName}' has {InstanceCount} healthy instances: {Endpoints}",
                            serviceName, instances.Count, string.Join(", ", serviceEndpoints[serviceName]));
                    }
                }

                _logger.LogInformation(" [YARP Built Service Endpoints] Built service endpoints map for {ServiceCount} services", serviceEndpoints.Count);
                return serviceEndpoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Error building fresh service endpoints from Consul");
                return new Dictionary<string, List<string>>();
            }
        }

        /// <summary>
        /// Try get cached endpoints từ Redis cache.
        /// </summary>
        /// <param name="cacheKey">Cache key để lookup</param>
        /// <returns>Cached endpoints hoặc null nếu cache miss</returns>
        private async Task<Dictionary<string, List<string>>?> TryGetCachedEndpointsAsync(string cacheKey)
        {
            try
            {
                // TODO: Implement Redis cache get
                return await _redisCacheService.GetAsync<Dictionary<string, List<string>>>(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[YARP Cache Error] Failed to get cached endpoints");
                return null;
            }
        }

        /// <summary>
        /// Try get stale cached endpoints cho fallback khi Consul fail.
        /// Sử dụng extended TTL từ config cho emergency scenarios.
        /// </summary>
        /// <param name="cacheKey">Cache key để lookup</param>
        /// <returns>Stale cached endpoints hoặc null</returns>
        private async Task<Dictionary<string, List<string>>?> TryGetStaleEndpointsAsync(string cacheKey)
        {
            try
            {
                // TODO: Implement stale cache với extended TTL từ config
                var staleKey = $"{_redisCacheConfig.StaleKeyPrefix}{cacheKey}";
                return await _redisCacheService.GetAsync<Dictionary<string, List<string>>>(staleKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[YARP Stale Cache Error] Failed to get stale cached endpoints");
                return null;
            }
        }

        /// <summary>
        /// Cache service endpoints vào Redis với configured TTL.
        /// </summary>
        /// <param name="cacheKey">Cache key để store</param>
        /// <param name="endpoints">Endpoints data để cache</param>
        private async Task CacheServiceEndpointsAsync(string cacheKey, Dictionary<string, List<string>> endpoints)
        {
            try
            {
                // TODO: Implement Redis cache set với TTL từ config
                var ttl = TimeSpan.FromSeconds(_redisCacheConfig.ServiceEndpointsTtlSeconds);
                await _redisCacheService.SetAsync(cacheKey, endpoints, ttl);

                // Đồng thời cache stale copy cho emergency fallback
                var staleKey = $"{_redisCacheConfig.StaleKeyPrefix}{cacheKey}";
                var staleTtl = TimeSpan.FromSeconds(_redisCacheConfig.StaleCacheTtlSeconds);
                await _redisCacheService.SetAsync(staleKey, endpoints, staleTtl);

                _logger.LogInformation("[YARP Cached] Cached {ServiceCount} service endpoints with TTL {TtlSeconds}s, key: {CacheKey}, stale key: {StaleKey}",
                    endpoints.Count, _redisCacheConfig.ServiceEndpointsTtlSeconds, cacheKey, staleKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[YARP Cache Error] Failed to cache service endpoints");
            }
        }

        

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Data transfer object để represent một service endpoint instance.
    /// Chứa thông tin cần thiết để connect đến service instance.
    /// </summary>
    /// <remarks>
    /// Class này được sử dụng để:
    /// - Store service instance information từ Consul
    /// - Debug và monitoring service health
    /// - Provide structured data cho logging và diagnostics
    /// </remarks>
    public class ServiceEndpoint
    {
        /// <summary>Tên của service</summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>IP address hoặc hostname của service instance</summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>Port number mà service instance đang listen</summary>
        public int Port { get; set; }

        /// <summary>Danh sách tags được assign cho service instance trong Consul</summary>
        public List<string> Tags { get; set; } = new();
    }

    #endregion
}