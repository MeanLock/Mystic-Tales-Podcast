using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.DataAccess.Entities;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;

namespace PodcastService.BusinessLogic.Services.DbServices.FilterServices
{
    public class FilterTagService
    {
        // LOGGER
        private readonly ILogger<FilterTagService> _logger;

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
        private readonly IGenericRepository<FilterTag> _filterTagGenericRepository;
        private readonly IGenericRepository<FilterTagType> _filterTagTypeGenericRepository;
        private readonly IGenericRepository<TakerTagFilter> _takerTagFilterGenericRepository;


        public FilterTagService(
            ILogger<FilterTagService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,
            IGenericRepository<Account> accountRepository,
            IGenericRepository<Role> roleRepository,

            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IGenericRepository<FilterTag> filterTagGenericRepository,
            IGenericRepository<FilterTagType> filterTagTypeGenericRepository,
            IGenericRepository<TakerTagFilter> takerTagFilterGenericRepository
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _unitOfWork = unitOfWork;

            _filterTagGenericRepository = filterTagGenericRepository;
            _filterTagTypeGenericRepository = filterTagTypeGenericRepository;
            _takerTagFilterGenericRepository = takerTagFilterGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;


            _appConfig = appConfig;
        }

        public async Task RegisterFilterTag(int accountId)
        {
            try
            {
                IEnumerable<FilterTagType> filterTagTypes = await _filterTagTypeGenericRepository.FindAll().ToListAsync();
                IEnumerable<FilterTag> filterTags = await _filterTagGenericRepository.FindAll().ToListAsync();
                foreach (var filterTagType in filterTagTypes)
                {
                    foreach (var filterTag in filterTags.Where(tag => tag.FilterTagTypeId == filterTagType.Id))
                    {
                        TakerTagFilter takerTagFilter = new TakerTagFilter
                        {
                            TakerId = accountId,
                            FilterTagId = filterTag.Id,
                            Summary = null, // Initialize with null or empty string if needed
                        };
                        // Console.WriteLine("\n\n\nTakerTagFilter: " + takerTagFilter.TakerId + " - " + takerTagFilter.FilterTagId + "\n\n\n");
                        await _takerTagFilterGenericRepository.CreateAsync(takerTagFilter);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lỗi đăng kí filter tags: " + ex.Message);
            }

        }

        
    }
}
