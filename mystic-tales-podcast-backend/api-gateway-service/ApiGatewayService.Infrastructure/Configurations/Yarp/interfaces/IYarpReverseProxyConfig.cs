namespace ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces
{
    public interface IYarpReverseProxyConfig
    {
        bool EnableCaching { get; set; }
        int DefaultCacheTtlMinutes { get; set; }
        bool EnableCompression { get; set; }
        int TimeoutSeconds { get; set; }
        long MaxRequestBodySize { get; set; }
        bool EnableRequestBuffering { get; set; }
        
        YarpLoadBalancingData LoadBalancing { get; set; }
        YarpHealthCheckData HealthCheck { get; set; }
    }

    public class YarpLoadBalancingData
    {
        public string DefaultPolicy { get; set; } = "RoundRobin";
        public int HealthCheckIntervalSeconds { get; set; } = 30;
        public int FailureThreshold { get; set; } = 3;
        public int RecoveryThreshold { get; set; } = 2;
        public bool EnableSessionAffinity { get; set; } = false;
        public string SessionAffinityFailurePolicy { get; set; } = "Redistribute";
        public int TimeoutMilliseconds { get; set; } = 30000;
    }

    public class YarpHealthCheckData
    {
        public bool EnableHealthChecks { get; set; } = true;
        public string HealthCheckPath { get; set; } = "/health";
        public int HealthCheckIntervalSeconds { get; set; } = 30;
        public int HealthCheckTimeoutSeconds { get; set; } = 5;
        public int UnhealthyThreshold { get; set; } = 3;
        public int HealthyThreshold { get; set; } = 2;
        public bool EnablePassiveHealthChecks { get; set; } = true;
        public string[] RequiredHeaders { get; set; } = Array.Empty<string>();
    }
}
