namespace TransactionService.Infrastructure.Configurations.Google.interfaces
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
        string? AccountForgotPassword_TemplateViewPath { get; set; }
        string? AccountVerification_TemplateViewPath { get; set; }
        string? AccountForgotPassword_MailSubject { get; set; }
        string? AccountVerification_MailSubject { get; set; }
    }
}
