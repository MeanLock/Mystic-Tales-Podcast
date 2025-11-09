using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.DataAccess.Data;
using UserService.DataAccess.UOW;
using UserService.DataAccess.Repositories.interfaces;
using UserService.DataAccess.Entities;
using Net.payOS;
using Net.payOS.Types;
using UserService.BusinessLogic.DTOs.Transaction;
using System.Linq.Expressions;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.DTOs.Feedback;
using UserService.Infrastructure.Configurations.Payos.interfaces;
using UserService.BusinessLogic.Helpers.AuthHelpers;
using UserService.BusinessLogic.Helpers.FileHelpers;

namespace UserService.BusinessLogic.Services.DbServices.MiscServices
{
    public class QueryMetricService
    {
        // LOGGER
        private readonly ILogger<QueryMetricService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IPayosConfig _payosConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        


        public QueryMetricService(
            ILogger<QueryMetricService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            
            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IPayosConfig payosConfig
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
            _payosConfig = payosConfig;
        }
       

        /////////////////////////////////////////////////////////////



    }
}
