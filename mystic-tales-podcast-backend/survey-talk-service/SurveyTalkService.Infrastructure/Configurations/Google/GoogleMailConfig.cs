using Microsoft.Extensions.Configuration;
using SurveyTalkService.Infrastructure.Configurations.Google.interfaces;

namespace SurveyTalkService.Infrastructure.Configurations.Google
{
    public class GoogleMailConfigModel
    {
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string? AccountForgotPassword_TemplateViewPath { get; set; }
        public string? AccountVerification_TemplateViewPath { get; set; }

        public string? AccountForgotPassword_MailSubject { get; set; }
        public string? AccountVerification_MailSubject { get; set; }
    }
    public class GoogleMailConfig : IGoogleMailConfig
    {
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string? AccountForgotPassword_TemplateViewPath { get; set; }
        public string? AccountVerification_TemplateViewPath { get; set; }

        public string? AccountForgotPassword_MailSubject { get; set; }
        public string? AccountVerification_MailSubject { get; set; }
        public GoogleMailConfig(IConfiguration configuration)
        {
            var googleMailConfig = configuration.GetSection("Infrastructure:Google:Mail").Get<GoogleMailConfigModel>();
            FromEmail = googleMailConfig?.FromEmail;
            FromName = googleMailConfig?.FromName;
            SmtpUsername = googleMailConfig?.SmtpUsername;
            SmtpPassword = googleMailConfig?.SmtpPassword;
            SmtpHost = googleMailConfig?.SmtpHost;
            SmtpPort = googleMailConfig?.SmtpPort ?? 0;
            EnableSsl = googleMailConfig?.EnableSsl ?? false;
            AccountForgotPassword_TemplateViewPath = googleMailConfig?.AccountForgotPassword_TemplateViewPath;
            AccountVerification_TemplateViewPath = googleMailConfig?.AccountVerification_TemplateViewPath;
            AccountForgotPassword_MailSubject = googleMailConfig?.AccountForgotPassword_MailSubject;
            AccountVerification_MailSubject = googleMailConfig?.AccountVerification_MailSubject;
        }
    }
}
