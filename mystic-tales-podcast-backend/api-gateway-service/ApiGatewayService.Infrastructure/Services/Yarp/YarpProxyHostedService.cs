using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces;

namespace ApiGatewayService.Infrastructure.Services.Yarp
{
    /// <summary>
    /// Background service chịu trách nhiệm quản lý và refresh YARP configuration từ Consul.
    /// Chạy như một hosted service để periodic update routing configuration.
    /// </summary>
    /// <remarks>
    /// Service này:
    /// - Khởi tạo initial YARP configuration từ Consul khi application start
    /// - Thực hiện periodic refresh configuration theo schedule
    /// - Chỉ hoạt động khi EnableDynamicRouting = true
    /// - Coordinate giữa YarpConfigurationService và YarpDynamicProxyConfigProvider
    /// - Graceful shutdown khi application stopping
    /// </remarks>
    public class YarpProxyHostedService : BackgroundService
    {
        #region Private Fields

        /// <summary>Service để build YARP configuration từ Consul</summary>
        private readonly YarpConfigurationService _yarpConfigurationService;
        
        /// <summary>Service discovery service để check service availability</summary>
        private readonly YarpServiceDiscoveryService _yarpServiceDiscoveryService;
        
        /// <summary>Dynamic config provider để update YARP runtime</summary>
        private readonly YarpDynamicProxyConfigProvider _dynamicProxyConfigProvider;
        
        /// <summary>Cấu hình routing behavior và refresh intervals</summary>
        private readonly IYarpRoutingConfig _yarpRoutingConfig;
        
        /// <summary>Logger để ghi log các hoạt động background service</summary>
        private readonly ILogger<YarpProxyHostedService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo YarpProxyHostedService với các dependencies cần thiết.
        /// </summary>
        /// <param name="yarpConfigurationService">Service để build YARP config</param>
        /// <param name="yarpServiceDiscoveryService">Service discovery service</param>
        /// <param name="dynamicProxyConfigProvider">Dynamic config provider</param>
        /// <param name="yarpRoutingConfig">Routing configuration</param>
        /// <param name="logger">Logger service</param>
        public YarpProxyHostedService(
            YarpConfigurationService yarpConfigurationService,
            YarpServiceDiscoveryService yarpServiceDiscoveryService,
            YarpDynamicProxyConfigProvider dynamicProxyConfigProvider,
            IYarpRoutingConfig yarpRoutingConfig,
            ILogger<YarpProxyHostedService> logger)
        {
            _yarpConfigurationService = yarpConfigurationService;
            _yarpServiceDiscoveryService = yarpServiceDiscoveryService;
            _dynamicProxyConfigProvider = dynamicProxyConfigProvider;
            _yarpRoutingConfig = yarpRoutingConfig;
            _logger = logger;
        }

        #endregion

        #region BackgroundService Implementation

        /// <summary>
        /// Main execution method cho background service.
        /// Thực hiện initial load và periodic refresh của YARP configuration.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token để graceful shutdown</param>
        /// <returns>Task completion</returns>
        /// <remarks>
        /// Workflow:
        /// 1. Check nếu dynamic routing enabled
        /// 2. Load initial configuration từ Consul
        /// 3. Setup periodic timer để refresh theo schedule
        /// 4. Continue refresh until application shutdown
        /// </remarks>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[YARP Starting] YARP Proxy HostedService starting...");

            // Check nếu dynamic routing được enable
            if (!_yarpRoutingConfig.EnableDynamicRouting)
            {
                _logger.LogInformation("==> Dynamic routing is disabled. YARP will use static configuration.");
                return;
            }

            // Load initial configuration từ Consul khi service start
            await RefreshProxyConfigurationAsync(stoppingToken);

            // Setup periodic refresh timer theo configured interval
            var refreshInterval = TimeSpan.FromSeconds(_yarpRoutingConfig.RouteRefreshIntervalSeconds);
            using var timer = new PeriodicTimer(refreshInterval);

            try
            {
                // Continue refresh loop until application shutdown
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RefreshProxyConfigurationAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception khi application shutdown
                _logger.LogInformation("YARP Proxy HostedService stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[YARP Error] Error in YARP Proxy HostedService execution");
            }

            _logger.LogInformation("YARP Proxy HostedService stopped");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refresh YARP proxy configuration từ Consul service discovery.
        /// Method này orchestrate toàn bộ quá trình refresh configuration.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token để early exit nếu application stopping</param>
        /// <returns>Task completion</returns>
        /// <remarks>
        /// Workflow:
        /// 1. Gọi YarpDynamicProxyConfigProvider.UpdateConfigAsync()
        /// 2. Provider sẽ gọi YarpConfigurationService để load fresh data
        /// 3. Update YARP runtime configuration
        /// 4. Handle exceptions và log appropriately
        /// </remarks>
        private async Task RefreshProxyConfigurationAsync(CancellationToken stoppingToken)
        {
            var retryCount = 0;
            var maxRetries = _yarpRoutingConfig.MaxRetryAttempts;

            while (retryCount <= maxRetries)
            {
                try
                {
                    _logger.LogDebug("[YARP Refreshing] Refreshing YARP configuration (attempt {AttemptNumber}/{MaxAttempts})", 
                        retryCount + 1, maxRetries + 1);

                    // Update YARP dynamic configuration via provider
                    await _dynamicProxyConfigProvider.UpdateConfigAsync(_yarpConfigurationService);

                    if (retryCount > 0)
                    {
                        _logger.LogInformation("[YARP Refreshed] YARP configuration refresh succeeded after {RetryCount} retries", retryCount);
                    }

                    // Success - break retry loop
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount > maxRetries)
                    {
                        _logger.LogError(ex, "[YARP Error] Failed to refresh YARP configuration after {MaxRetries} attempts", maxRetries);
                        break;
                    }

                    _logger.LogWarning(ex, "[YARP Warn] Error refreshing YARP configuration (attempt {AttemptNumber}/{MaxAttempts}). Retrying in {DelaySeconds} seconds...", 
                        retryCount, maxRetries + 1, _yarpRoutingConfig.RefreshRetryDelaySeconds);

                    await Task.Delay(TimeSpan.FromSeconds(_yarpRoutingConfig.RefreshRetryDelaySeconds), stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[YARP Stopping] YARP Proxy HostedService is stopping...");
            await base.StopAsync(stoppingToken);
        }

        #endregion
    }
}