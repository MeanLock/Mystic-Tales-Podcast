namespace ModerationService.Infrastructure.Configurations.Google.interfaces
{
    public interface IGoogleMailConfig
    {
        string? FromEmail { get; set; }
        string? FromName { get; set; }
        string? SmtpUsername { get; set; }
        string? SmtpPassword { get; set; }
        string? SmtpHost { get; set; }
        int SmtpPort { get; set; }
        bool EnableSsl { get; set; }
    }
}
