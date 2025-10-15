using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TransactionService.Common.AppConfigurations.App.interfaces;
using TransactionService.Common.AppConfigurations.FilePath.interfaces;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Common.AppConfigurations.BusinessSetting.interfaces;
using TransactionService.Infrastructure.Services.Google.Email;

namespace TransactionService.BusinessLogic.Services.DbServices.MiscServices
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

                public async Task SendTransactionServiceEmail(MailProperty mailProperty, string toEmail, object viewModel)
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
