namespace BookingManagementService.Infrastructure.Configurations.Google.interfaces
{
    public interface IGoogleOAuth2Config
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
    }
}
