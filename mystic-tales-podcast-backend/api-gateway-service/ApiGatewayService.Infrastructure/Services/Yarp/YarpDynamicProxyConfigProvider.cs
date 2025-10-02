using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiGatewayService.Infrastructure.Services.Yarp
{
    /// <summary>
    /// Custom implementation của IProxyConfigProvider cho YARP dynamic configuration.
    /// Cung cấp runtime configuration updates mà không cần restart application.
    /// </summary>
    /// <remarks>
    /// Provider này implement YARP's IProxyConfigProvider interface để:
    /// - Cung cấp initial empty configuration khi khởi tạo
    /// - Cho phép update configuration tại runtime thông qua UpdateConfig()
    /// - Signal configuration changes đến YARP runtime thông qua change tokens
    /// - Integrate với YarpConfigurationService để refresh từ Consul
    /// </remarks>
    public class YarpDynamicProxyConfigProvider : IProxyConfigProvider
    {
        #region Private Fields

        /// <summary>
        /// Current YARP configuration (thread-safe với volatile keyword).
        /// Chứa routes và clusters được load từ Consul service discovery.
        /// </summary>
        private volatile DynamicProxyConfig _config;
        
        /// <summary>Logger để ghi log các hoạt động configuration</summary>
        private readonly ILogger<YarpDynamicProxyConfigProvider> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo provider với empty configuration.
        /// Configuration sẽ được update sau khi YarpConfigurationService load từ Consul.
        /// </summary>
        /// <param name="logger">Logger service</param>
        public YarpDynamicProxyConfigProvider(ILogger<YarpDynamicProxyConfigProvider> logger)
        {
            _logger = logger;
            // Khởi tạo với empty configuration - sẽ được update sau khi load từ Consul
            _config = new DynamicProxyConfig(Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
        }

        #endregion

        #region IProxyConfigProvider Implementation

        /// <summary>
        /// Trả về current YARP configuration.
        /// Method này được YARP runtime gọi để lấy routes và clusters.
        /// </summary>
        /// <returns>Current proxy configuration</returns>
        public IProxyConfig GetConfig() => _config;

        #endregion

        #region Public Methods

        /// <summary>
        /// Update YARP configuration với routes và clusters mới.
        /// Thread-safe method để update configuration tại runtime.
        /// </summary>
        /// <param name="routes">Danh sách route configurations mới</param>
        /// <param name="clusters">Danh sách cluster configurations mới</param>
        /// <remarks>
        /// Method này:
        /// - Tạo DynamicProxyConfig mới với routes/clusters
        /// - Signal change event để YARP runtime reload configuration
        /// - Thread-safe thông qua volatile field assignment
        /// </remarks>
        public void UpdateConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            try
            {
                // Store reference to old config trước khi update
                var oldConfig = _config;
                
                // Tạo new configuration với fresh routes và clusters
                _config = new DynamicProxyConfig(routes, clusters);
                
                // Signal change event để YARP runtime biết configuration đã thay đổi
                oldConfig.SignalChange();

                _logger.LogInformation("[YARP Updated] YARP dynamic configuration updated: {RouteCount} routes, {ClusterCount} clusters", 
                    routes.Count, clusters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Failed to update YARP dynamic configuration");
            }
        }

        /// <summary>
        /// Async method để refresh configuration từ YarpConfigurationService.
        /// Được gọi bởi YarpProxyHostedService để periodic refresh.
        /// </summary>
        /// <param name="configurationService">Configuration service để load fresh data từ Consul</param>
        /// <returns>Task completion</returns>
        /// <remarks>
        /// Method này orchestrate toàn bộ quá trình refresh:
        /// - Gọi YarpConfigurationService.GetProxyConfigAsync() để lấy fresh data
        /// - Gọi UpdateConfig() để apply configuration changes
        /// - Handle exceptions và log appropriately
        /// </remarks>
        public async Task UpdateConfigAsync(YarpConfigurationService configurationService)
        {
            try
            {
                _logger.LogInformation("[YARP Refreshing] Refreshing YARP dynamic configuration from Consul...");

                // Lấy fresh configuration từ Consul thông qua YarpConfigurationService
                var (routes, clusters) = await configurationService.GetProxyConfigAsync();

                // Apply configuration changes
                UpdateConfig(routes, clusters);
                _logger.LogInformation("[YARP Refreshed] YARP dynamic configuration updated: {RouteCount} routes, {ClusterCount} clusters", _config.Routes.Count, _config.Clusters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Failed to refresh YARP dynamic configuration from Consul");
            }
        }

        #endregion
    }

    #region Internal Helper Classes

    /// <summary>
    /// Internal implementation của IProxyConfig để wrap routes và clusters.
    /// Cung cấp change token mechanism cho YARP runtime.
    /// </summary>
    /// <remarks>
    /// Class này implement IProxyConfig interface với:
    /// - Routes: Danh sách route configurations
    /// - Clusters: Danh sách cluster configurations  
    /// - ChangeToken: Token để signal configuration changes đến YARP
    /// - SignalChange(): Method để trigger configuration reload
    /// </remarks>
    internal class DynamicProxyConfig : IProxyConfig
    {
        /// <summary>CancellationTokenSource để tạo change tokens</summary>
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// Khởi tạo DynamicProxyConfig với routes và clusters.
        /// </summary>
        /// <param name="routes">Route configurations</param>
        /// <param name="clusters">Cluster configurations</param>
        public DynamicProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        /// <summary>Danh sách route configurations cho YARP</summary>
        public IReadOnlyList<RouteConfig> Routes { get; }
        
        /// <summary>Danh sách cluster configurations cho YARP</summary>
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        
        /// <summary>Change token để signal configuration changes đến YARP runtime</summary>
        public IChangeToken ChangeToken { get; }

        /// <summary>
        /// Signal configuration change để trigger YARP runtime reload.
        /// Method này cancel current change token, khiến YARP reload configuration.
        /// </summary>
        internal void SignalChange()
        {
            _cts.Cancel();
        }
    }

    #endregion
}
