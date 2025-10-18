using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SurveyTalkService.BusinessLogic.Helpers.AuthHelpers;
using SurveyTalkService.BusinessLogic.Helpers.FileHelpers;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;
using SurveyTalkService.Common.AppConfigurations.FilePath.interfaces;
using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.UOW;

namespace SurveyTalkService.BusinessLogic.Services.DbServices.UserServices
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
