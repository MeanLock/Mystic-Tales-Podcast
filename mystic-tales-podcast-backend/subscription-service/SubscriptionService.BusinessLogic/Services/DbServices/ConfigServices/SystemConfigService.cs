using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscriptionService.BusinessLogic.Helpers.AuthHelpers;
using SubscriptionService.BusinessLogic.Helpers.FileHelpers;
using SubscriptionService.Common.AppConfigurations.App.interfaces;
using SubscriptionService.Common.AppConfigurations.FilePath.interfaces;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.UOW;

namespace SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices
{
    public class SystemConfigService
    {
        // LOGGER
        private readonly ILogger<SystemConfigService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;


        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES



        public SystemConfigService(
            ILogger<SystemConfigService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _unitOfWork = unitOfWork;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;


            _appConfig = appConfig;
        }

        


    }
}
