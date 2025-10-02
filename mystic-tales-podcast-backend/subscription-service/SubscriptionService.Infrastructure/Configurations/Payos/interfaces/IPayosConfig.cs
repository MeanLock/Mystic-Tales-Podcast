namespace SubscriptionService.Infrastructure.Configurations.Payos.interfaces
{
    public interface IPayosConfig
    {
        string ClientID { get; set; }
        string APIKey { get; set; }
        string ChecksumKey { get; set; }
    }
}
