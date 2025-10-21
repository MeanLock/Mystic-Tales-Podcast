using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Services.DbServices.ReportServices
{
    public class PodcastBuddyReportService
    {
        private readonly IGenericRepository<PodcastBuddyReport> _podcastBuddyReportGenericRepository; 

        private readonly AccountCachingService _accountCachingService;

        private readonly ILogger<PodcastBuddyReportService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public PodcastBuddyReportService(
            IGenericRepository<PodcastBuddyReport> podcastBuddyReportGenericRepository,
            AccountCachingService accountCachingService,
            ILogger<PodcastBuddyReportService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _podcastBuddyReportGenericRepository = podcastBuddyReportGenericRepository;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<PodcastBuddyReportListItemResponseDTO>> GetAllPodcastReport()
        {
            var query = await _podcastBuddyReportGenericRepository.FindAll(
                predicate: null,
                includeFunc: source => source
                    .Include(r => r.PodcastBuddyReportType)
                ).ToListAsync();
            var podcastBuddyReport = (await Task.WhenAll(query.Select(async pbr =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pbr.PodcastBuddyId);
                    return new PodcastBuddyReportListItemResponseDTO()
                    {
                        Id = pbr.Id,
                        Content = pbr.Content,
                        AccountId = pbr.AccountId,
                        PodcastBuddy = new PodcastBuddySnippetDTO()
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.FullName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        PodcastBuddyReportType = new PodcastBuddyReportTypeDTO()
                        {
                            Id = pbr.PodcastBuddyReportType.Id,
                            Name = pbr.PodcastBuddyReportType.Name,
                        },
                        ResolvedAt = pbr.ResolvedAt,
                        CreatedAt = pbr.CreatedAt,
                    };
                }))).ToList();
            return podcastBuddyReport;
        }
    }
}
