using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Services.Google.Email;
using PodcastService.Infrastructure.Configurations.Google.interfaces;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Helpers.DateHelpers;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.Infrastructure.Models.Kafka;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Infrastructure.Services.Redis;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel;
using PodcastService.BusinessLogic.Enums.Podcast;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Episode.Details;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UploadEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitEpisodeAudioFile;
using PodcastService.Infrastructure.Services.Audio.Transcription;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodeDraftAudio;
using PodcastService.BusinessLogic.Services.AudioServices;
using PodcastService.Infrastructure.Services.Audio.AcoustID;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using Newtonsoft.Json;
using PodcastService.BusinessLogic.Enums.Account;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequestEpisodeAudioExamination;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodePublishAudio;
using PodcastService.Infrastructure.Models.Audio.Hls;
using PodcastService.Infrastructure.Services.Audio.Hls;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel;
using HotChocolate.Execution.Processing;
using PodcastService.BusinessLogic.DTOs.ReviewSession.ListItems;
using PodcastService.BusinessLogic.DTOs.ReviewSession.Details;
using PodcastService.BusinessLogic.DTOs.ReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequireEpisodePublishReviewSessionEdit;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.AcceptEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RejectEpisodePublishReviewSession;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class HashtagService
    {
        // LOGGER
        private readonly ILogger<HashtagService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IPodcastPublishReviewSessionConfig _podcastPublishReviewSessionConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Hashtag> _hashtagGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        // CACHING SERVICE
        private readonly AccountCachingService _accountCachingService;

        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        // KAFKA SERVICE
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;

        // REDIS SERVICE
        private readonly RedisSharedCacheService _redisSharedCacheService;

        // AUDIO SERVICE
        private readonly AudioTranscriptionService _audioTranscriptionService;
        private readonly AcoustIDAudioFingerprintGenerator _acoustIDAudioFingerprintGenerator;
        private readonly AcoustIDAudioFingerprintComparator _acoustIDAudioFingerprintComparator;
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;


        public HashtagService(
            ILogger<HashtagService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            FluentEmailService fluentEmailService,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<Hashtag> hashtagGenericRepository,
            FileIOHelper fileIOHelper,
            DateHelper dateHelper,

            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IPodcastPublishReviewSessionConfig podcastPublishReviewSessionConfig,

            HttpServiceQueryClient httpServiceQueryClient,

            AccountCachingService accountCachingService,

            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService,

            AudioTranscriptionService audioTranscriptionService,
            AcoustIDAudioFingerprintGenerator audioFingerprintService,
            AcoustIDAudioFingerprintComparator audioFingerprintComparator,
            FFMpegCoreHlsService ffMpegCoreHlsService
            )
        {
            _logger = logger;

            _appDbContext = appDbContext;
            _unitOfWork = unitOfWork;

            _hashtagGenericRepository = hashtagGenericRepository;

            _fileIOHelper = fileIOHelper;
            _jwtHelper = jwtHelper;
            _bcryptHelper = bcryptHelper;
            _dateHelper = dateHelper;

            _fluentEmailService = fluentEmailService;

            _filePathConfig = filePathConfig;
            _podcastPublishReviewSessionConfig = podcastPublishReviewSessionConfig;
            _appConfig = appConfig;

            _httpServiceQueryClient = httpServiceQueryClient;

            _accountCachingService = accountCachingService;

            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;

            _audioTranscriptionService = audioTranscriptionService;
            _acoustIDAudioFingerprintGenerator = audioFingerprintService;
            _acoustIDAudioFingerprintComparator = audioFingerprintComparator;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
        }


        public async Task<List<HashtagDTO>> GetAllHashtagsByKeywordAsync(string keyword)
        {
            keyword = keyword.Trim();
            var hashtagEntities = await _hashtagGenericRepository.FindAll(
                predicate: ht => EF.Functions.Like(ht.Name, $"%{keyword}%"),
                includeFunc: null
            ).ToListAsync();

            var hashtagModels = hashtagEntities.Select(ht => new HashtagDTO
            {
                Id = ht.Id,
                Name = ht.Name
            }).ToList();

            return hashtagModels;
        }

        public async Task<HashtagDTO> CreateHashtagAsync(HashtagCreateRequestDTO hashtagCreateRequestDTO)
        {
            using(var transaction = await  _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    hashtagCreateRequestDTO.HashtagName = hashtagCreateRequestDTO.HashtagName.Trim();
                    // kiểm tra trùng tên hashtag không ignore case
                    var existingHashtag = await _hashtagGenericRepository.FindAll(
                        predicate: ht => ht.Name == hashtagCreateRequestDTO.HashtagName,
                        includeFunc: null
                    ).FirstOrDefaultAsync();

                    if (existingHashtag != null)
                    {
                        return new HashtagDTO
                        {
                            Id = existingHashtag.Id,
                            Name = existingHashtag.Name
                        };
                    }

                    var newHashtag = new Hashtag
                    {
                        Name = hashtagCreateRequestDTO.HashtagName,
                    };
                    await _hashtagGenericRepository.CreateAsync(newHashtag);

                    await transaction.CommitAsync();

                    var newHashtagDTO = new HashtagDTO
                    {
                        Id = newHashtag.Id,
                        Name = newHashtag.Name
                    };
                    return newHashtagDTO;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Failed to create hashtag, please try again later.");
                }
            }
        }


    }
}