namespace Architecture_1.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisDefaultConfig
    {
        string ConnectionString { get; set; }
        string InstanceName { get; set; }
        int DefaultDatabase { get; set; }
        int ConnectTimeout { get; set; }
        int SyncTimeout { get; set; }
        bool AbortOnConnectFail { get; set; }
        int ConnectRetry { get; set; }
        bool UseSsl { get; set; }
        string Password { get; set; }
        

    }
}
