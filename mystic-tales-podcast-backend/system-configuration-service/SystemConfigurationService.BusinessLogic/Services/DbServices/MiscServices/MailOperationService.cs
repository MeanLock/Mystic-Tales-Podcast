using Microsoft.Extensions.Logging;
using SystemConfigurationService.Common.AppConfigurations.App.interfaces;
using SystemConfigurationService.Common.AppConfigurations.FilePath.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.Payos.interfaces;
using SystemConfigurationService.Common.AppConfigurations.BusinessSetting.interfaces;
using SystemConfigurationService.Infrastructure.Services.Google.Email;

namespace SystemConfigurationService.BusinessLogic.Services.DbServices.MiscServices
{
    public class MailOperationService
    {
        // LOGGER
        private readonly ILogger<MailOperationService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IPayosConfig _payosConfig;
        
        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;
        


        public MailOperationService(
            ILogger<MailOperationService> logger,
            FluentEmailService fluentEmailService,
            IAppConfig appConfig,
            IFilePathConfig filePathConfig,
            IPayosConfig payosConfig
            )
        {
            _logger = logger;
            _fluentEmailService = fluentEmailService;
            _appConfig = appConfig;
            _filePathConfig = filePathConfig;
            _payosConfig = payosConfig;
        }

        /////////////////////////////////////////////////////////////

                public async Task SendSystemConfigurationServiceEmail(MailProperty mailProperty, string toEmail, object viewModel)
        {
            try
            {
                await _fluentEmailService.SendEmail(toEmail, viewModel, mailProperty.TemplateFilePath
                , mailProperty.Subject);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Send email failed, error: " + ex.Message);
            }
        }

    }
}
