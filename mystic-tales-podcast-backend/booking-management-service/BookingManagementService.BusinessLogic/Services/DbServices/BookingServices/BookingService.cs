using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using BookingManagementService.BusinessLogic.DTOs.Account;
using BookingManagementService.BusinessLogic.DTOs.Auth;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Booking.Details;
using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.Cache;
using BookingManagementService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AcceptBookingDealing;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AddPodcastTonesToPodcaster;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeBookingNegotitation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelPodcasterBookingsTerminatePodcasterForce;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteAllUserBookingProducingListenSessions;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingDealing;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingDepositPayment;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingPayTheRestPayment;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RemovePodcasterBookingTracksListenSessionTerminatePodcasterForce;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.TerminateBookingOfPodcaster;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.UpdateBookingListenSessionDuration;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;
using BookingManagementService.BusinessLogic.DTOs.Report;
using BookingManagementService.BusinessLogic.DTOs.Snippet;
using BookingManagementService.BusinessLogic.DTOs.SystemConfiguration;
using BookingManagementService.BusinessLogic.DTOs.Transaction;
using BookingManagementService.BusinessLogic.Enums;
using BookingManagementService.BusinessLogic.Enums.Account;
using BookingManagementService.BusinessLogic.Enums.App;
using BookingManagementService.BusinessLogic.Enums.Booking;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Enums.ListenSessionProcedure;
using BookingManagementService.BusinessLogic.Enums.Transaction;
using BookingManagementService.BusinessLogic.Helpers.DateHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.DbServices.CachingServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.Infrastructure.Configurations.Audio.Hls.interfaces;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Kafka;
using Duende.IdentityServer.Extensions;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BookingManagementService.BusinessLogic.Services.DbServices.BookingServices
{
    public class BookingService
    {
        private readonly IGenericRepository<Booking> _bookingGenericRepository;
        private readonly IGenericRepository<BookingStatusTracking> _bookingStatusTrackingGenericRepository;
        private readonly IGenericRepository<BookingProducingRequest> _bookingProducingRequestGenericRepository;
        private readonly IGenericRepository<BookingRequirement> _bookingRequirementGenericRepository;
        private readonly IGenericRepository<BookingChatRoom> _bookingChatRoomGenericRepository;
        private readonly IGenericRepository<BookingChatMember> _bookingChatMemberGenericRepository;
        private readonly IGenericRepository<PodcastBookingTone> _podcastBookingToneGenericRepository;
        private readonly IGenericRepository<BookingPodcastTrack> _bookingPodcastTrackGenericRepository;
        private readonly IGenericRepository<PodcastBuddyBookingTone> _podcastBuddyBookingToneGenericRepository;
        private readonly IGenericRepository<BookingPodcastTrackListenSession> _bookingPodcastTrackListenSessionGenericRepository;
        private readonly IGenericRepository<BookingOptionalManualCancelReason> _bookingOptionalManualCancelReasonGenericRepository;
        private readonly IPodcastBuddyBookingToneRepository _podcastBuddyBookingToneRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly AccountCachingService _accountCachingService;
        private readonly CustomerListenSessionProcedureCachingService _customerListenSessionProcedureCachingService;
        private readonly ILogger<BookingService> _logger;
        private readonly IHlsConfig _hlsConfig;

        private readonly AppDbContext _appDbContext;
        private readonly ICustomerListenSessionProcedureConfig _customerListenSessionProcedureConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IBookingListenSessionConfig _bookingListenSessionConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;
        public BookingService(
            IGenericRepository<Booking> bookingGenericRepository,
            IGenericRepository<BookingStatusTracking> bookingStatusTrackingGenericRepository,
            IGenericRepository<BookingProducingRequest> bookingProducingRequestGenericRepository,
            IGenericRepository<BookingRequirement> bookingRequirementGenericRepository,
            IGenericRepository<BookingChatRoom> bookingChatRoomGenericRepository,
            IGenericRepository<BookingChatMember> bookingChatMemberGenericRepository,
            IGenericRepository<PodcastBookingTone> podcastBookingToneGenericRepository,
            IGenericRepository<BookingPodcastTrack> bookingPodcastTrackGenericRepository,
            IGenericRepository<PodcastBuddyBookingTone> podcastBuddyBookingToneGenericRepository,
            IGenericRepository<BookingPodcastTrackListenSession> bookingPodcastTrackListenSessionGenericRepository,
            IGenericRepository<BookingOptionalManualCancelReason> bookingOptionalManualCancelReasonGenericRepository,
            IPodcastBuddyBookingToneRepository podcastBuddyBookingToneRepository,
            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            AccountCachingService accountCachingService,
            CustomerListenSessionProcedureCachingService customerListenSessionProcedureCachingService,
            ILogger<BookingService> logger,
            IHlsConfig hlsConfig,
            AppDbContext appDbContext,
            ICustomerListenSessionProcedureConfig customerListenSessionProcedureConfig,
            IFilePathConfig filePathConfig,
            IBookingListenSessionConfig bookingListenSessionConfig,
            FileIOHelper fileIOHelper,
            DateHelper dateHelper
            )
        {
            _bookingGenericRepository = bookingGenericRepository;
            _bookingStatusTrackingGenericRepository = bookingStatusTrackingGenericRepository;
            _bookingProducingRequestGenericRepository = bookingProducingRequestGenericRepository;
            _bookingRequirementGenericRepository = bookingRequirementGenericRepository;
            _bookingChatRoomGenericRepository = bookingChatRoomGenericRepository;
            _bookingChatMemberGenericRepository = bookingChatMemberGenericRepository;
            _podcastBookingToneGenericRepository = podcastBookingToneGenericRepository;
            _bookingPodcastTrackGenericRepository = bookingPodcastTrackGenericRepository;
            _podcastBuddyBookingToneGenericRepository = podcastBuddyBookingToneGenericRepository;
            _bookingPodcastTrackListenSessionGenericRepository = bookingPodcastTrackListenSessionGenericRepository;
            _podcastBuddyBookingToneRepository = podcastBuddyBookingToneRepository;
            _bookingOptionalManualCancelReasonGenericRepository = bookingOptionalManualCancelReasonGenericRepository;

            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _accountCachingService = accountCachingService;
            _customerListenSessionProcedureCachingService = customerListenSessionProcedureCachingService;
            _logger = logger;
            _hlsConfig = hlsConfig;
            _appDbContext = appDbContext;
            _customerListenSessionProcedureConfig = customerListenSessionProcedureConfig;
            _filePathConfig = filePathConfig;
            _bookingListenSessionConfig = bookingListenSessionConfig;
            _fileIOHelper = fileIOHelper;
            _dateHelper = dateHelper;
        }
        public async Task<List<BookingListItemResponseDTO>> GetAllBookingsAsync(int accountId, int roleId)
        {
            try
            {
                var bookings = await _bookingGenericRepository.FindAll(
                    includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings)
                        .ThenInclude(bs => bs.BookingStatus))
                    .Where(b => roleId == (int)RoleEnum.Staff ? b.AssignedStaffId == accountId : true)
                    .ToListAsync();

                var result = new List<BookingListItemResponseDTO>();

                foreach (var booking in bookings)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(booking.PodcastBuddyId);
                    AccountStatusCache? assignedStaff = null;
                    if (booking.AssignedStaffId != null)
                    {
                        assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                    }
                    result.Add(new BookingListItemResponseDTO
                    {
                        Id = booking.Id,
                        Title = booking.Title,
                        Description = booking.Description,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastBuddy = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                        {
                            Id = assignedStaff.Id,
                            FullName = assignedStaff.FullName,
                            Email = assignedStaff.Email,
                            MainImageFileKey = assignedStaff.MainImageFileKey
                        } : null,
                        DeadlineDays = booking.DeadlineDays,
                        Price = booking.Price,
                        Deadline = booking.Deadline,
                        DemoAudioFileKey = booking.DemoAudioFileKey,
                        BookingManualCancelledReason = booking.BookingManualCancelledReason,
                        BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                        CreatedAt = booking.CreatedAt,
                        UpdatedAt = booking.UpdatedAt,
                        CurrentStatus = new BookingStatusResponseDTO
                        {
                            Id = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Id,
                            Name = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Name
                        }
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all bookings");
                throw new HttpRequestException($"Retrieving all Bookings failed. Error: {ex.Message}");
            }
        }
        public async Task<BookingDetailResponseDTO?> GetBookingByIdAsync(int bookingId, int accountId, int roleId)
        {
            try
            {
                var booking = await _bookingGenericRepository.FindByIdAsync(
                    bookingId,
                    includeFunc: function => function
                    .Include(b => b.BookingRequirements)
                        .ThenInclude(br => br.PodcastBookingTone)
                        .ThenInclude(bt => bt.PodcastBookingToneCategory)
                    .Include(b => b.BookingProducingRequests)
                        .ThenInclude(bp => bp.BookingPodcastTracks)
                    .Include(b => b.BookingStatusTrackings)
                        .ThenInclude(bst => bst.BookingStatus));

                if (booking == null)
                    return null;

                if (roleId == (int)RoleEnum.Customer && !(booking.AccountId == accountId || booking.PodcastBuddyId == accountId))
                {
                    throw new HttpRequestException("Account not authorize to view the detail of this booking");
                }
                if (roleId == (int)RoleEnum.Staff && booking.AssignedStaffId != accountId)
                {
                    throw new HttpRequestException("Account not authorize to view the detail of this booking");
                }

                var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                var podcastBuddy = await GetPodcaster(booking.PodcastBuddyId);
                AccountStatusCache? assignedStaff = null;
                if (booking.AssignedStaffId != null)
                {
                    assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                }

                if (podcastBuddy == null)
                {
                    throw new HttpRequestException("PodcastBuddy not found");
                }

                var latestStatus = booking.BookingStatusTrackings?
                    .OrderByDescending(bst => bst.CreatedAt)
                    .FirstOrDefault();

                var lastestProducingRequest = booking.BookingProducingRequests?.Where(bp => bp.FinishedAt != null).OrderByDescending(bp => bp.CreatedAt).FirstOrDefault();

                var lastestBookingPodcastTracks = new List<BookingPodcastTrackListItemResponseDTO>();
                if (lastestProducingRequest != null)
                {
                    lastestBookingPodcastTracks = lastestProducingRequest.BookingPodcastTracks.Select(bpt => new BookingPodcastTrackListItemResponseDTO
                    {
                        Id = bpt.Id,
                        BookingId = bpt.BookingId,
                        BookingRequirementId = bpt.BookingRequirementId,
                        BookingProducingRequestId = bpt.BookingProducingRequestId,
                        AudioFileKey = bpt.AudioFileKey,
                        AudioFileSize = bpt.AudioFileSize,
                        AudioLength = bpt.AudioLength,
                        RemainingPreviewListenSlot = bpt.RemainingPreviewListenSlot
                    }).ToList();
                }

                return new BookingDetailResponseDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    Account = new AccountSnippetResponseDTO
                    {
                        Id = account.Id,
                        FullName = account.FullName,
                        Email = account.Email,
                        MainImageFileKey = account.MainImageFileKey
                    },
                    PodcastBuddy = new PodcastBuddySnippetResponseDTO
                    {
                        Id = podcastBuddy.Id,
                        FullName = podcastBuddy.PodcasterProfile.Name,
                        Email = podcastBuddy.Email,
                        Description = podcastBuddy.PodcasterProfile?.Description ?? string.Empty,
                        MainImageFileKey = podcastBuddy.MainImageFileKey,
                        AverageRating = podcastBuddy.PodcasterProfile?.AverageRating ?? 0,
                        RatingCount = podcastBuddy.PodcasterProfile?.RatingCount ?? 0,
                        TotalFollow = podcastBuddy.PodcasterProfile?.TotalFollow ?? 0,
                        TotalBookingCompleted = 0,
                        PriceBookingPerWord = podcastBuddy.PodcasterProfile?.PricePerBookingWord ?? 0
                    },
                    AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                    {
                        Id = assignedStaff.Id,
                        FullName = assignedStaff.FullName,
                        Email = assignedStaff.Email,
                        MainImageFileKey = assignedStaff.MainImageFileKey
                    } : null,
                    Price = booking.Price,
                    Deadline = booking.Deadline,
                    DeadlineDays = booking.DeadlineDays,
                    DemoAudioFileKey = booking.DemoAudioFileKey,
                    BookingManualCancelledReason = booking.BookingManualCancelledReason,
                    BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                    CreatedAt = booking.CreatedAt,
                    UpdatedAt = booking.UpdatedAt,
                    BookingRequirementFileList = booking.BookingRequirements?.Select(re => new BookingRequirementListItemResponseDTO
                    {
                        Id = re.Id,
                        BookingId = re.BookingId,
                        Name = re.Name,
                        Description = re.Description,
                        RequirementDocumentFileKey = re.RequirementDocumentFileKey,
                        Order = re.Order,
                        WordCount = re.WordCount,
                        PodcastBookingTone = new PodcastBookingToneDetailResponseDTO()
                        {
                            Id = re.PodcastBookingTone.Id,
                            Name = re.PodcastBookingTone.Name,
                            Description = re.PodcastBookingTone.Description ?? string.Empty,
                            CreatedAt = re.PodcastBookingTone.CreatedAt,
                            DeletedAt = re.PodcastBookingTone.DeletedAt,
                            PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO()
                            {
                                Id = re.PodcastBookingTone.PodcastBookingToneCategory.Id,
                                Name = re.PodcastBookingTone.PodcastBookingToneCategory.Name
                            }
                        }
                    }).ToList() ?? new List<BookingRequirementListItemResponseDTO>(),
                    BookingProducingRequestList = booking.BookingProducingRequests?.OrderByDescending(bp => bp.CreatedAt).Select(prod => new BookingProducingRequestListItemResponseDTO
                    {
                        Id = prod.Id,
                        BookingId = prod.BookingId,
                        Note = prod.Note,
                        Deadline = prod.Deadline,
                        DeadlineDays = prod.DeadlineDays,
                        RejectReason = prod.RejectReason,
                        IsAccepted = prod.IsAccepted,
                        FinishedAt = prod.FinishedAt,
                        CreatedAt = prod.CreatedAt
                    }).ToList() ?? new List<BookingProducingRequestListItemResponseDTO>(),
                    CurrentStatus = new BookingStatusResponseDTO
                    {
                        Id = latestStatus?.BookingStatus?.Id ?? 0,
                        Name = latestStatus?.BookingStatus?.Name ?? "Unknown"
                    },
                    StatusTracking = booking.BookingStatusTrackings?.Select(bst => new BookingStatusTrackingListItemResponseDTO
                    {
                        Id = bst.Id,
                        BookingId = bst.BookingId,
                        BookingStatusId = bst.BookingStatusId,
                        CreatedAt = bst.CreatedAt
                    }).OrderByDescending(bst => bst.CreatedAt).ToList() ?? new List<BookingStatusTrackingListItemResponseDTO>(),
                    LastestBookingPodcastTracks = lastestBookingPodcastTracks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking with ID: {BookingId}", bookingId);
                throw new HttpRequestException($"Retrieving Booking failed. Error: {ex.Message}");
            }
        }
        public async Task<List<PodcastBookingToneMeListItemResponseDTO>?> GetAllPodcastBookingTonesByPodcasterIdAsync(int podcastBuddyId)
        {
            try
            {
                List<PodcastBookingToneMeListItemResponseDTO> result = new List<PodcastBookingToneMeListItemResponseDTO>();
                var podcastBookingTones = await _podcastBuddyBookingToneGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(pb => pb.PodcastBookingTone)
                    .ThenInclude(pb => pb.PodcastBookingToneCategory))
                    .Where(pb => pb.PodcasterId == podcastBuddyId)
                    .Select(pb => new PodcastBookingToneMeListItemResponseDTO
                    {
                        Id = pb.PodcastBookingTone.Id,
                        Name = pb.PodcastBookingTone.Name,
                        Description = pb.PodcastBookingTone.Description,
                        PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO
                        {
                            Id = pb.PodcastBookingTone.PodcastBookingToneCategory.Id,
                            Name = pb.PodcastBookingTone.PodcastBookingToneCategory.Name
                        },
                    })
                    .ToListAsync();
                if (podcastBookingTones != null)
                {
                    result = podcastBookingTones;
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving podcast booking tones for PodcasterId: {podcastBuddyId}", podcastBuddyId);
                throw new HttpRequestException($"Retrieving Podcast Booking Tones for PodcasterId {podcastBuddyId} failed. Error: {ex.Message}");
            }
        }
        public async Task<List<PodcastBookingToneMeListItemResponseDTO>> GetPodcasterPodcastBookingTonesAsync(int accountId)
        {
            try
            {
                List<PodcastBookingToneMeListItemResponseDTO> result = new List<PodcastBookingToneMeListItemResponseDTO>();
                var podcastBookingTones = await _podcastBuddyBookingToneGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(pb => pb.PodcastBookingTone)
                    .ThenInclude(pb => pb.PodcastBookingToneCategory))
                    .Where(pb => pb.PodcasterId == accountId)
                    .Select(pb => new PodcastBookingToneMeListItemResponseDTO
                    {
                        Id = pb.PodcastBookingTone.Id,
                        Name = pb.PodcastBookingTone.Name,
                        Description = pb.PodcastBookingTone.Description,
                        PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO
                        {
                            Id = pb.PodcastBookingTone.PodcastBookingToneCategory.Id,
                            Name = pb.PodcastBookingTone.PodcastBookingToneCategory.Name
                        },
                    })
                    .ToListAsync();
                if (podcastBookingTones != null)
                {
                    result = podcastBookingTones;
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving podcast booking tones for PodcasterId: {AccountId}", accountId);
                throw new HttpRequestException($"Retrieving Podcast Booking Tones for PodcasterId {accountId} failed. Error: {ex.Message}");
            }
        }
        public async Task CreateBookingAsync(CreateBookingParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var processedFiles = new List<string>();
                var isTransactionCommit = false;
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(parameter.PodcastBuddyId);
                    if (podcaster == null)
                    {
                        throw new HttpRequestException($"PodcastBuddy with Id {parameter.PodcastBuddyId} not found");
                    }
                    if (!podcaster.HasVerifiedPodcasterProfile || !podcaster.PodcasterProfileIsBuddy)
                    {
                        _logger.LogError($"PodcasterProfile: {podcaster.HasVerifiedPodcasterProfile}. PodcastBuddy: {podcaster.PodcasterProfileIsBuddy}. PodcastVerified: {podcaster.PodcasterProfileIsVerified}");
                        throw new HttpRequestException($"Podcaster with Id {parameter.PodcastBuddyId} does not qualify to request booking");
                    }
                    //if (!podcaster.HasVerifiedPodcasterProfile)
                    //{
                    //    _logger.LogError($"PodcasterProfile: {podcaster.HasVerifiedPodcasterProfile}. PodcastBuddy: {podcaster.PodcasterProfileIsBuddy}. PodcastVerified: {podcaster.PodcasterProfileIsVerified}");
                    //    throw new HttpRequestException($"Podcaster with Id {parameter.PodcastBuddyId} does not qualify to request booking");
                    //}
                    if (parameter.DeadlineDayCount == 0)
                    {
                        throw new HttpRequestException("Deadline day count can not be zero");
                    }

                    var newBooking = new Booking
                    {
                        Title = parameter.Title,
                        Description = parameter.Description,
                        AccountId = parameter.AccountId,
                        PodcastBuddyId = parameter.PodcastBuddyId,
                        Deadline = null,
                        DeadlineDays = parameter.DeadlineDayCount,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var createdBooking = await _bookingGenericRepository.CreateAsync(newBooking);

                    if (createdBooking == null)
                    {
                        throw new HttpRequestException("Failed to create booking - returned null");
                    }

                    var createdRequirementDocumentList = new List<BookingRequirement>();

                    Console.WriteLine("________________________________________________________");
                    Console.WriteLine(parameter.BookingRequirementInfoList.Count());

                    // Process each track
                    foreach (var requirementDocumentInfo in parameter.BookingRequirementInfoList)
                    {
                        var result = await _podcastBuddyBookingToneGenericRepository.FindAll()
                            .Where(pbbt => pbbt.PodcasterId == parameter.PodcastBuddyId
                                && pbbt.PodcastBookingToneId == requirementDocumentInfo.PodcastBookingToneId)
                            .FirstOrDefaultAsync();
                        if (result == null)
                        {
                            throw new HttpRequestException($"PodcastBuddy with Id {parameter.PodcastBuddyId} does not have the PodcastBookingTone with Id {requirementDocumentInfo.PodcastBookingToneId}");
                        }
                        var newBookingRequirement = new BookingRequirement()
                        {
                            BookingId = createdBooking.Id,
                            Name = requirementDocumentInfo.Name,
                            Description = requirementDocumentInfo.Description,
                            Order = requirementDocumentInfo.Order,
                            PodcastBookingToneId = requirementDocumentInfo.PodcastBookingToneId,
                            RequirementDocumentFileKey = ""
                        };

                        var createdBookingRequirement = await _bookingRequirementGenericRepository.CreateAsync(newBookingRequirement);

                        if (createdBookingRequirement == null)
                        {
                            throw new HttpRequestException("Failed to create booking requirement - returned null");
                        }

                        var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + createdBooking.Id;
                        if (requirementDocumentInfo.RequirementDocumentFileKey != null && requirementDocumentInfo.RequirementDocumentFileKey != "")
                        {
                            var requirementDocumentFileKey = FilePathHelper.CombinePaths(folderPath, $"{createdBookingRequirement.Id}_requirement_document{FilePathHelper.GetExtension(requirementDocumentInfo.RequirementDocumentFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(requirementDocumentInfo.RequirementDocumentFileKey, requirementDocumentFileKey);
                            processedFiles.Add(requirementDocumentFileKey);
                            await _fileIOHelper.DeleteFileAsync(requirementDocumentInfo.RequirementDocumentFileKey);
                            createdBookingRequirement.RequirementDocumentFileKey = requirementDocumentFileKey;
                            await _bookingRequirementGenericRepository.UpdateAsync(createdBookingRequirement.Id, createdBookingRequirement);
                        }

                        createdRequirementDocumentList.Add(createdBookingRequirement);
                    }

                    await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking
                    {
                        //Id = Guid.NewGuid(),
                        BookingId = newBooking.Id,
                        BookingStatusId = (int)BookingStatusEnum.QuotationRequest,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    });

                    await transaction.CommitAsync();
                    isTransactionCommit = true;

                    var newResponseData = new JObject
                    {
                        { "BookingId", newBooking.Id },
                        { "Title", newBooking.Title },
                        { "Description", newBooking.Description },
                        { "AccountId", newBooking.AccountId },
                        { "PodcastBuddyId", newBooking.PodcastBuddyId },
                        { "DeadlineDays", newBooking.DeadlineDays },
                        // { "CreatedBookingRequirementDocumentList", JArray.FromObject(createdRequirementDocumentList) },
                        { "CreatedAt", newBooking.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Booking created successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    if (isTransactionCommit == false)
                    {
                        await transaction.RollbackAsync();
                    }

                    foreach (var processedFile in processedFiles)
                    {
                        try
                        {
                            await _fileIOHelper.DeleteFileAsync(processedFile);
                            _logger.LogInformation("Cleaned up processed file: {FilePath}", processedFile);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "Failed to clean up processed file: {FilePath}", processedFile);
                        }
                    }

                    _logger.LogError(ex, "Error occurred while creating booking for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Create booking failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking created failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task<List<BookingListItemResponseDTO>> GetBookingsByPodcasterIdAsync(int podcastBuddyId)
        {
            try
            {
                var bookings = await _bookingGenericRepository.FindAll(
                    includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings)
                        .ThenInclude(bs => bs.BookingStatus))
                    .Where(b => b.PodcastBuddyId == podcastBuddyId)
                    .ToListAsync();

                var result = new List<BookingListItemResponseDTO>();

                foreach (var booking in bookings)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(booking.PodcastBuddyId);
                    AccountStatusCache? assignedStaff = null;
                    if (booking.AssignedStaffId != null)
                    {
                        assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                    }
                    result.Add(new BookingListItemResponseDTO
                    {
                        Id = booking.Id,
                        Title = booking.Title,
                        Description = booking.Description,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastBuddy = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                        {
                            Id = assignedStaff.Id,
                            FullName = assignedStaff.FullName,
                            Email = assignedStaff.Email,
                            MainImageFileKey = assignedStaff.MainImageFileKey
                        } : null,
                        DeadlineDays = booking.DeadlineDays,
                        Price = booking.Price,
                        Deadline = booking.Deadline,
                        DemoAudioFileKey = booking.DemoAudioFileKey,
                        BookingManualCancelledReason = booking.BookingManualCancelledReason,
                        BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                        CreatedAt = booking.CreatedAt,
                        UpdatedAt = booking.UpdatedAt,
                        CurrentStatus = new BookingStatusResponseDTO
                        {
                            Id = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Id,
                            Name = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Name
                        }
                    });
                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving bookings for PodcasterId: {PodcastBuddyId}", podcastBuddyId);
                throw new HttpRequestException($"Retrieving Bookings for PodcasterId {podcastBuddyId} failed. Error: {ex.Message}");
            }
        }
        public async Task<List<string>> GetOptionalManualCancelReasonsAsync()
        {
            try
            {
                var result = await _bookingOptionalManualCancelReasonGenericRepository
                    .FindAll()
                    .Select(bomc => bomc.Name).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving optional manual cancel reasons");
                throw new HttpRequestException($"Retrieving optional manual cancel reasons failed. Error: {ex.Message}");
            }
        }
        public async Task<List<BookingListItemResponseDTO>> GetBookingsByAccountIdAsync(int accountId, bool isPodcaster)
        {
            try
            {
                var bookings = await _bookingGenericRepository.FindAll(
                    includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings)
                        .ThenInclude(bs => bs.BookingStatus))
                    .Where(b => isPodcaster ? b.PodcastBuddyId == accountId : b.AccountId == accountId)
                    .ToListAsync();

                var result = new List<BookingListItemResponseDTO>();

                foreach (var booking in bookings)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(booking.PodcastBuddyId);
                    AccountStatusCache? assignedStaff = null;
                    if (booking.AssignedStaffId != null)
                    {
                        assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                    }
                    result.Add(new BookingListItemResponseDTO
                    {
                        Id = booking.Id,
                        Title = booking.Title,
                        Description = booking.Description,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastBuddy = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                        {
                            Id = assignedStaff.Id,
                            FullName = assignedStaff.FullName,
                            Email = assignedStaff.Email,
                            MainImageFileKey = assignedStaff.MainImageFileKey
                        } : null,
                        Price = booking.Price,
                        Deadline = booking.Deadline,
                        DeadlineDays = booking.DeadlineDays,
                        DemoAudioFileKey = booking.DemoAudioFileKey,
                        BookingManualCancelledReason = booking.BookingManualCancelledReason,
                        BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                        CreatedAt = booking.CreatedAt,
                        UpdatedAt = booking.UpdatedAt,
                        CurrentStatus = new BookingStatusResponseDTO
                        {
                            Id = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Id,
                            Name = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Name
                        }
                    });
                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving bookings for AccountId: {AccountId}", accountId);
                throw new HttpRequestException($"Retrieving Bookings for AccountId {accountId} failed. Error: {ex.Message}");
            }
        }
        public async Task<List<PodcastBookingToneListItemResponseDTO>> GetAllPodcastBookingTonesAsync()
        {
            try
            {
                var result = new List<PodcastBookingToneListItemResponseDTO>();
                var query = await _podcastBookingToneGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(pbt => pbt.PodcastBookingToneCategory))
                    .ToListAsync();
                var list = await _podcastBuddyBookingToneGenericRepository.FindAll()
                        .ToListAsync();
                return result = (await Task.WhenAll(query.Select(async pbt =>
                {
                    var count = 0;
                    var availablePodcasterCount = list
                        .Where(pbbt => pbbt.PodcastBookingToneId == pbt.Id)
                        .Select(pbbt => pbbt.PodcasterId)
                        .Distinct()
                        .ToList();
                    foreach (var podcasterId in availablePodcasterCount)
                    {
                        var podcasterAccount = await _accountCachingService.GetAccountStatusCacheById(podcasterId);
                        if (podcasterAccount != null && podcasterAccount.HasVerifiedPodcasterProfile && podcasterAccount.PodcasterProfileIsBuddy && podcasterAccount.IsVerified)
                        {
                            count++;
                        }
                    }
                    return new PodcastBookingToneListItemResponseDTO
                    {
                        Id = pbt.Id,
                        Name = pbt.Name,
                        Description = pbt.Description,
                        PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO
                        {
                            Id = pbt.PodcastBookingToneCategory.Id,
                            Name = pbt.PodcastBookingToneCategory.Name
                        },
                        AvailablePodcasterCount = count,
                        CreatedAt = pbt.CreatedAt,
                        DeletedAt = pbt.DeletedAt
                    };
                }))).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving bookings tone");
                throw new HttpRequestException($"Retrieving Bookings tones failed. Error: {ex.Message}");
            }
        }
        public async Task<List<BookingRequirementListItemResponseDTO>> GetAllBookingRequirementByBookingIdAsync(int bookingId)
        {
            try
            {
                return await _bookingRequirementGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(br => br.PodcastBookingTone)
                    .ThenInclude(pbt => pbt.PodcastBookingToneCategory))
                    .Where(br => br.BookingId == bookingId)
                    .Select(br => new BookingRequirementListItemResponseDTO
                    {
                        Id = br.Id,
                        BookingId = bookingId,
                        Name = br.Name,
                        Description = br.Description,
                        Order = br.Order,
                        RequirementDocumentFileKey = br.RequirementDocumentFileKey,
                        WordCount = br.WordCount,
                        PodcastBookingTone = new PodcastBookingToneDetailResponseDTO
                        {
                            Id = br.PodcastBookingTone.Id,
                            Name = br.PodcastBookingTone.Name,
                            Description = br.PodcastBookingTone.Description,
                            PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO
                            {
                                Id = br.PodcastBookingTone.PodcastBookingToneCategory.Id,
                                Name = br.PodcastBookingTone.PodcastBookingToneCategory.Name
                            },
                            CreatedAt = br.PodcastBookingTone.CreatedAt,
                            DeletedAt = br.PodcastBookingTone.DeletedAt
                        }
                    }).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking requirements for BookingId: {BookingId}", bookingId);
                throw new HttpRequestException($"Retrieving Booking requirements for BookingId {bookingId} failed. Error: {ex.Message}");
            }
        }
        public async Task RejectBookingAsync(RejectBookingParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var bookingId = parameter.BookingId;
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingId,
                        includeFunc: function => function
                            .Include(b => b.BookingStatusTrackings)
                    );

                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }

                    var isValid = await ValidateBookingPodcasterAsync(bookingId, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new Exception("You are not authorized to reject this booking.");
                    }

                    var currentStatus = booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;
                    if (currentStatus < (int)BookingStatusEnum.QuotationRejected)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            //Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            BookingStatusId = (int)BookingStatusEnum.QuotationRejected,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                    }
                    else
                    {
                        throw new Exception("Booking is not in a valid status to be rejected");
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingId", bookingId },
                        { "UpdatedAt", booking.UpdatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Booking rejected successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while rejecting booking for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Reject booking failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking rejection failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task ManualCancelBookingAsync(CancelBookingManualParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var bookingId = parameter.BookingId;
                    var bookingManualCancelledReason = parameter.BookingManualCancelledReason;
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var systemConfig = await GetActiveSystemConfigProfile();
                    if (systemConfig == null)
                    {
                        throw new Exception("System config not found");
                    }

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingId,
                        includeFunc: function => function
                            .Include(b => b.BookingStatusTrackings)
                            .Include(b => b.BookingProducingRequests)
                    );

                    var isValid = await ValidateBookingAccountAsync(booking.Id, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new Exception("You are not authorized to cancel this booking.");
                    }

                    if (booking != null)
                    {
                        var currentStatus = booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;
                        var newBookingStatusId = 0;
                        if (currentStatus < (int)BookingStatusEnum.QuotationRejected)
                        {
                            if (parameter.AccountId == booking.AccountId)
                                newBookingStatusId = (int)BookingStatusEnum.QuotationCancelled;
                            else if (parameter.AccountId == booking.PodcastBuddyId)
                                newBookingStatusId = (int)BookingStatusEnum.QuotationRejected;
                            else
                                throw new Exception("Only customer or podcast buddy can manually cancel booking in Quotation status");
                        }
                        else if (currentStatus == (int)BookingStatusEnum.Producing)
                        {
                            if (booking.AccountId != parameter.AccountId)
                                throw new Exception("Only customer can manually cancel booking in Producing status");
                            if (booking.BookingProducingRequests.Count() == 1 && booking.BookingProducingRequests.Select(b => b.Deadline).FirstOrDefault() <= _dateHelper.GetNowByAppTimeZone())
                                newBookingStatusId = (int)BookingStatusEnum.CancelledManually;
                            else
                                throw new Exception("Cannot manually cancel booking in Producing. Condition not met (The first producing request and pass deadline)");
                        }
                        else
                        {
                            throw new Exception("Invalid booking status for manual cancellation");
                        }

                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            Id = Guid.NewGuid(),
                            BookingId = bookingId,
                            BookingStatusId = newBookingStatusId,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);

                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingManualCancelledReason = bookingManualCancelledReason;
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                        if (newBookingStatusId == (int)BookingStatusEnum.CancelledManually)
                        {
                            if (booking.Price == null)
                                throw new Exception("Booking Price is null, cannot process refund");
                            var profitRate = systemConfig.BookingConfig.ProfitRate;
                            var depositRate = systemConfig.BookingConfig.DepositRate;

                            if (booking.AccountId == parameter.AccountId)
                            {
                                var Amount = booking.Price * (decimal)depositRate;
                                var refundMessageName = "booking-refund-flow";
                                var newRequestData = new JObject
                                {
                                    { "BookingId", booking.Id },
                                    { "Profit", null },
                                    { "Amount", Amount },
                                    { "AccountId", booking.AccountId },
                                    { "PodcasterId", booking.PodcastBuddyId },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
                                };
                                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: newRequestData,
                                    sagaInstanceId: null,
                                    messageName: refundMessageName);
                                await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, sagaId.ToString());
                                _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", startSagaTriggerMessage.SagaInstanceId);
                            }
                            //else
                            //{
                            //    var Amount = booking.Price * (decimal)depositRate ;
                            //    var refundMessageName = "booking-deposit-compensation-flow";
                            //    var newRequestData = new JObject
                            //    {
                            //        { "BookingId", booking.Id },
                            //        { "Profit", null },
                            //        { "Amount", Amount },
                            //        { "AccountId", booking.AccountId },
                            //        { "PodcasterId", booking.AccountId },
                            //        { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
                            //    };
                            //    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.PaymentProcessingDomain,
                            //        requestData: newRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: refundMessageName);
                            //    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, sagaId.ToString());
                            //    _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", startSagaTriggerMessage.SagaInstanceId);
                            //}
                        }

                        await transaction.CommitAsync();

                        var newResponseData = new JObject
                        {
                            { "BookingId", booking.Id },
                            { "BookingManualCancelledReason", booking.BookingManualCancelledReason},
                            { "UpdatedAt", booking.UpdatedAt }
                        };
                        var newMessageName = messageName + ".success";
                        var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                            topic: KafkaTopicEnum.BookingManagementDomain,
                            requestData: command.RequestData,
                            responseData: newResponseData,
                            sagaInstanceId: sagaId,
                            flowName: flowName,
                            messageName: newMessageName);
                        await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                        _logger.LogInformation("Booking cancel successfully for SagaId: {SagaId}", command.SagaInstanceId);
                    }
                    else
                    {
                        //_logger.LogError("Booking not found or Price is null for SagaId: {SagaId}", command.SagaInstanceId);
                        throw new HttpRequestException("Booking not found");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while cancelling booking for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Cancel booking failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking cancel failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        //public async Task CreateBookingNegotiationAsync(CreateBookingNegotiationParameterDTO parameter, SagaCommandMessage command)
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var messageName = command.MessageName;
        //            var sagaId = command.SagaInstanceId;
        //            var flowName = command.FlowName;
        //            var responseData = command.LastStepResponseData;
        //            var bookingNegotitationId = Guid.NewGuid();

        //            var booking = await _bookingGenericRepository.FindByIdWithPaths(
        //                parameter.BookingId,
        //                "BookingStatusTrackings"
        //            );

        //            if (booking == null)
        //            {
        //                throw new Exception($"Booking with ID {parameter.BookingId} not found");
        //            }

        //            if(booking.AccountId != parameter.AccountId && booking.PodcastBuddyId != parameter.AccountId)
        //            {
        //                throw new Exception("Only customer or podcast buddy can create negotiation");
        //            }

        //            bool isFromCustomer = booking.AccountId == parameter.AccountId;

        //            var newBookingNegotiation = new BookingNegotiation()
        //            {
        //                Id = bookingNegotitationId,
        //                BookingId = parameter.BookingId,
        //                Note = parameter.Note ?? string.Empty,
        //                Deadline = parameter.Deadline,
        //                Price = parameter.Price,
        //                DemoAudioRequired = parameter.DemoAudioRequired ?? false,
        //                DemoAudioFileKey = null,
        //                IsCompleted = false,
        //                IsFromCustomer = isFromCustomer,
        //                CreatedAt = _dateHelper.GetNowByAppTimeZone()
        //            };
        //            await _bookingNegotiationGenericRepository.CreateAsync(newBookingNegotiation);

        //            var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + newBookingNegotiation.BookingId;
        //            if (!string.IsNullOrEmpty(parameter.DemoAudioFileKey) && !isFromCustomer)
        //            {
        //                var DemoAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"{bookingNegotitationId}_negotiation_demo_audio{FilePathHelper.GetExtension(parameter.DemoAudioFileKey)}");
        //                await _fileIOHelper.CopyFileToFileAsync(parameter.DemoAudioFileKey, DemoAudioFileKey);
        //                await _fileIOHelper.DeleteFileAsync(parameter.DemoAudioFileKey);
        //                newBookingNegotiation.DemoAudioFileKey = DemoAudioFileKey;
        //                await _bookingNegotiationGenericRepository.UpdateAsync(newBookingNegotiation.Id, newBookingNegotiation);
        //            }

        //            // Fix concurrency issue by handling updates one by one
        //            var oldNegotiations = _bookingNegotiationGenericRepository.FindAll()
        //                .Where(bn => bn.BookingId == parameter.BookingId && bn.IsCompleted == false && bn.Id != newBookingNegotiation.Id)
        //                .ToList();

        //            foreach (var bn in oldNegotiations)
        //            {
        //                bn.IsCompleted = true;
        //                await _bookingNegotiationGenericRepository.UpdateAsync(bn.Id, bn);
        //            }

        //            var currentStatus = booking.BookingStatusTrackings?.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;

        //            if (currentStatus == (int)BookingStatusEnum.QuotationDealing && newBookingNegotiation.IsFromCustomer)
        //            {
        //                var newBookingStatusTracking = new BookingStatusTracking
        //                {
        //                    Id = Guid.NewGuid(),
        //                    BookingId = booking.Id,
        //                    BookingStatusId = (int)BookingStatusEnum.QuotationUnderNegotiation,
        //                    CreatedAt = _dateHelper.GetNowByAppTimeZone(),
        //                };
        //                await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
        //                booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //                await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
        //            }
        //            else if(currentStatus == (int)BookingStatusEnum.QuotationUnderNegotiation && !newBookingNegotiation.IsFromCustomer)
        //            {
        //                var newBookingStatusTracking = new BookingStatusTracking
        //                {
        //                    Id = Guid.NewGuid(),
        //                    BookingId = booking.Id,
        //                    BookingStatusId = (int)BookingStatusEnum.QuotationDealing,
        //                    CreatedAt = _dateHelper.GetNowByAppTimeZone(),
        //                };
        //                await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
        //                booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //                await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
        //            }
        //            else
        //            {
        //                throw new Exception("Cannot create negotiation in the current booking status");
        //            }

        //            await transaction.CommitAsync();

        //            var newResponseData = new JObject
        //            {
        //                { "BookingNegotiationId", newBookingNegotiation.Id },
        //                { "CreatedAt", newBookingNegotiation.CreatedAt }
        //            };
        //            var newMessageName = messageName + ".success";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.BookingManagementDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: sagaId,
        //                flowName: flowName,
        //                messageName: newMessageName);
        //            var result = await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
        //            _logger.LogInformation("Booking negotiation created successfully for SagaId: {SagaId}", command.SagaInstanceId);

        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while creating booking negotiation for SagaId: {SagaId}", command.SagaInstanceId);

        //            if (!string.IsNullOrEmpty(parameter.DemoAudioFileKey))
        //            {
        //                await _fileIOHelper.DeleteFileAsync(parameter.DemoAudioFileKey);
        //            }

        //            var newResponseData = new JObject
        //                {
        //                    { "ErrorMessage", "Create booking negotiation failed, error: " + ex.Message },
        //                };
        //            var newMessageName = command.MessageName + ".failed";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.BookingManagementDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: command.SagaInstanceId,
        //                flowName: command.FlowName,
        //                messageName: newMessageName);
        //            var result = await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
        //            _logger.LogError("Booking negotiation created failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
        //        }
        //    }
        //}
        //public async Task AgreeBookingNegotiationAsync(AgreeBookingNegotiationParameterDTO parameter, SagaCommandMessage command)
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var messageName = command.MessageName;
        //            var sagaId = command.SagaInstanceId;
        //            var flowName = command.FlowName;
        //            var responseData = command.LastStepResponseData;

        //            var booking = await _bookingGenericRepository.FindByIdWithPaths(
        //                parameter.BookingId,
        //                "BookingNegotiations",
        //                "BookingStatusTrackings"
        //            );

        //            if (booking?.BookingStatusTrackings?.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId != (int)BookingStatusEnum.QuotationDealing)
        //            {
        //                throw new Exception("Cannot agree on negotiation when booking is rejected");
        //            }

        //            var latestNegotiation = booking.BookingNegotiations?.OrderByDescending(bn => bn.CreatedAt).FirstOrDefault();
        //            if (latestNegotiation == null)
        //            {
        //                throw new Exception("No negotiations found for this booking");
        //            }

        //            var bookingNegotiation = await _bookingNegotiationGenericRepository.FindByIdAsync(latestNegotiation.Id);

        //            if (bookingNegotiation == null)
        //            {
        //                throw new Exception("Booking negotiation not found");
        //            }

        //            bookingNegotiation.IsCompleted = true;

        //            await _bookingNegotiationGenericRepository.UpdateAsync(bookingNegotiation.Id, bookingNegotiation);

        //            booking.Price = bookingNegotiation.Price;
        //            booking.Deadline = bookingNegotiation.Deadline;

        //            if (!string.IsNullOrEmpty(bookingNegotiation.DemoAudioFileKey))
        //            {
        //                var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + booking.Id;
        //                var DemoAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"demo_audio{FilePathHelper.GetExtension(bookingNegotiation.DemoAudioFileKey)}");
        //                await _fileIOHelper.CopyFileToFileAsync(bookingNegotiation.DemoAudioFileKey, DemoAudioFileKey);
        //                await _fileIOHelper.DeleteFileAsync(bookingNegotiation.DemoAudioFileKey);
        //                booking.DemoAudioFileKey = DemoAudioFileKey;
        //            }

        //            booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //            await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
        //            var newBookingStatusTracking = new BookingStatusTracking
        //            {
        //                Id = Guid.NewGuid(),
        //                BookingId = booking.Id,
        //                BookingStatusId = (int)BookingStatusEnum.Producing,
        //                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
        //            };
        //            await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);

        //            await _bookingProducingRequestGenericRepository.CreateAsync(new BookingProducingRequest
        //            {
        //                Id = Guid.NewGuid(),
        //                BookingId = booking.Id,
        //                Note = bookingNegotiation.Note,
        //                Deadline = booking.Deadline ?? default,
        //                IsAccepted = true,
        //                FinishedAt = _dateHelper.GetNowByAppTimeZone(),
        //                CreatedAt = _dateHelper.GetNowByAppTimeZone()
        //            });

        //            var chatRoom = await _bookingChatRoomGenericRepository.CreateAsync(new BookingChatRoom
        //            {
        //                Id = Guid.NewGuid(),
        //                BookingId = booking.Id,
        //                CreatedAt = _dateHelper.GetNowByAppTimeZone()
        //            });

        //            if (chatRoom != null)
        //            {
        //                await _bookingChatMemberGenericRepository.CreateAsync(new BookingChatMember
        //                {
        //                    ChatRoomId = chatRoom.Id,
        //                    AccountId = booking.AccountId
        //                });
        //                await _bookingChatMemberGenericRepository.CreateAsync(new BookingChatMember
        //                {
        //                    ChatRoomId = chatRoom.Id,
        //                    AccountId = booking.PodcastBuddyId,
        //                });
        //            }

        //            await transaction.CommitAsync();

        //            var newResponseData = new JObject
        //            {
        //                { "BookingId", booking.Id },
        //                { "UpdatedAt", booking.UpdatedAt }
        //            };
        //            var newMessageName = messageName + ".success";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.BookingManagementDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: sagaId,
        //                flowName: flowName,
        //                messageName: newMessageName);
        //            await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
        //            _logger.LogInformation("Booking negotiation agreed successfully for SagaId: {SagaId}", command.SagaInstanceId);

        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while agreeing booking negotiation for SagaId: {SagaId}", command.SagaInstanceId);
        //            var newResponseData = new JObject
        //            {
        //                { "ErrorMessage", "Agree booking negotiation failed, error: " + ex.Message }
        //            };
        //            var newMessageName = command.MessageName + ".failed";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.BookingManagementDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: command.SagaInstanceId,
        //                flowName: command.FlowName,
        //                messageName: newMessageName);
        //            await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
        //            _logger.LogError("Booking negotiation agree failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
        //        }
        //    }
        //}
        public async Task ProcessBookingDealingAsync(ProcessBookingDealingParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var bookingId = parameter.BookingId;
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var requestData = command.RequestData;

                    var isValid = await ValidateBookingPodcasterAsync(bookingId, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new Exception("The logged in account are not authorized to deal with this booking.");
                    }

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingId,
                        includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings));
                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }
                    if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId != (int)BookingStatusEnum.QuotationRequest)
                    {
                        throw new Exception("Cannot deal booking that is not in 'Quotation Request' status.");
                    }

                    var podcaster = await GetPodcaster(booking.PodcastBuddyId);
                    if (podcaster == null)
                    {
                        throw new Exception($"Podcaster with ID {booking.PodcastBuddyId} not found");
                    }
                    var totalWordCount = 0;
                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        //Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        BookingStatusId = (int)BookingStatusEnum.QuotationDealing,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);

                    foreach (var reqInfo in parameter.BookingRequirementInfoList)
                    {
                        var bookingRequirement = await _bookingRequirementGenericRepository.FindByIdAsync(reqInfo.Id);
                        if (bookingRequirement != null)
                        {
                            totalWordCount += reqInfo.WordCount;
                            bookingRequirement.WordCount = reqInfo.WordCount;
                            await _bookingRequirementGenericRepository.UpdateAsync(bookingRequirement.Id, bookingRequirement);
                        }
                    }

                    booking.Price = totalWordCount * podcaster.PodcasterProfile.PricePerBookingWord;
                    if (parameter.DeadlineDayCount != null)
                    {
                        booking.DeadlineDays = parameter.DeadlineDayCount;
                    }
                    booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                    await transaction.CommitAsync();
                    var newResponseData = requestData;
                    newResponseData["UpdatedAt"] = booking.UpdatedAt;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Dealing booking successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while dealing booking for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Dealing booking failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Dealing booking failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }

        public async Task AcceptBookingDealingAsync(AcceptBookingDealingParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var bookingId = parameter.BookingId;
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingId,
                        includeFunc: function => function
                            .Include(b => b.BookingStatusTrackings)
                    );
                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }
                    if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId != (int)BookingStatusEnum.QuotationDealing)
                    {
                        throw new Exception("Cannot accept dealing for booking that is not in 'Quotation Dealing' status.");
                    }
                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        //Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        BookingStatusId = (int)BookingStatusEnum.Producing,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                    booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    booking.Deadline = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone().AddDays((double)booking.DeadlineDays.Value));
                    var staffList = await GetStaffList();
                    booking.AssignedStaffId = await GetRandomStaffFromList(staffList);
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                    var newBookingProducingRequest = new BookingProducingRequest
                    {
                        //Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        Note = string.Empty,
                        Deadline = _dateHelper.GetNowByAppTimeZone().AddDays((double)booking.DeadlineDays.Value),
                        IsAccepted = true,
                        FinishedAt = null,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _bookingProducingRequestGenericRepository.CreateAsync(newBookingProducingRequest);
                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "BookingId", bookingId },
                        { "UpdatedAt", booking.UpdatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Accepting booking dealing successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while accepting booking dealing for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Accepting booking dealing failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Accepting booking dealing failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }

        public async Task CompleteBookingAsync(CompleteBookingParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var bookingId = parameter.BookingId;
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingId,
                        includeFunc: function => function
                            .Include(b => b.BookingStatusTrackings)
                    );

                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }

                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        //Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        BookingStatusId = (int)BookingStatusEnum.Completed,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);

                    booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingId", bookingId },
                        { "UpdatedAt", booking.UpdatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Booking completed successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while completing booking for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Complete booking failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking completion failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        //public async Task BookingDayResponseAllowedChecking()
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var systemConfig = await GetActiveSystemConfigProfile();
        //            var previewResponseAllowedDays = systemConfig.BookingConfig.PreviewResponseAllowedDays;
        //            var producingRequestResponseAllowedDays = systemConfig.BookingConfig.ProducingRequestResponseAllowedDays;
        //            var profitRate = systemConfig.BookingConfig.ProfitRate;
        //            var depositRate = systemConfig.BookingConfig.DepositRate;


        //            var currentDateTime = _dateHelper.GetNowByAppTimeZone();
        //            var previewingBookingList = _bookingGenericRepository.FindAll()
        //                .Include(b => b.BookingProducingRequests)
        //                .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.TrackPreviewing &&
        //                b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt.HasValue &&
        //                b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt!.Value.AddDays(previewResponseAllowedDays) < currentDateTime)
        //                .ToList();

        //            var producingRequestBookingList = _bookingGenericRepository.FindAll()
        //                .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.ProducingRequested &&
        //                b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().IsAccepted == null &&
        //                b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().CreatedAt.AddDays(producingRequestResponseAllowedDays) < currentDateTime)
        //                .Include(b => b.BookingProducingRequests)
        //                .ToList();
        //            foreach (var booking in previewingBookingList)
        //            {
        //                var newBookingStatusTracking = new BookingStatusTracking
        //                {
        //                    Id = Guid.NewGuid(),
        //                    BookingId = booking.Id,
        //                    BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
        //                    CreatedAt = _dateHelper.GetNowByAppTimeZone(),
        //                };
        //                await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
        //                booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //                booking.BookingAutoCancelReason = "ExpiredPreview (quá thời hạn preview và pay the rest)";
        //                await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

        //                if (booking.Price.HasValue)
        //                {
        //                    var Amount = booking.Price * (decimal)depositRate - booking.Price * (decimal)profitRate;
        //                    var compensationMessageName = "booking-deposit-compenstation-flow";
        //                    var newRequestData = new JObject
        //                    {
        //                        { "BookingId", booking.Id },
        //                        { "Amount", Amount },
        //                        { "AccountId", booking.AccountId },
        //                        { "PodcasterId", booking.PodcastBuddyId },
        //                        { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
        //                    };
        //                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                        requestData: newRequestData,
        //                        sagaInstanceId: null,
        //                        messageName: compensationMessageName);
        //                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, booking.Id.ToString());
        //                    _logger.LogInformation("Booking deposit compensation message send successfully for BookingId: {BookingId}", booking.Id);
        //                }
        //            }
        //            foreach (var booking in producingRequestBookingList)
        //            {
        //                var newBookingStatusTracking = new BookingStatusTracking
        //                {
        //                    Id = Guid.NewGuid(),
        //                    BookingId = booking.Id,
        //                    BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
        //                    CreatedAt = _dateHelper.GetNowByAppTimeZone(),
        //                };
        //                await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
        //                booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //                booking.BookingAutoCancelReason = "Reason: PodcastBuddyNoResponse (không phản hồi producing request)";
        //                await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

        //                var startFirstSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                    topic: KafkaTopicEnum.UserManagementDomain,
        //                    requestData: new JObject
        //                    {
        //                        { "AccountId", booking.PodcastBuddyId },
        //                        { "ViolationPoint", 1 },
        //                    },
        //                    sagaInstanceId: null,
        //                    messageName: "user-violation-punishment-flow");
        //                await _messagingService.SendSagaMessageAsync(startFirstSagaTriggerMessage, booking.Id.ToString());
        //                _logger.LogInformation("User violation punishment message send successfully to AccountId: {AccountId} for BookingId: {BookingId}", booking.PodcastBuddyId, booking.Id);

        //                if (booking.Price.HasValue)
        //                {
        //                    var Amount = booking.Price * (decimal)depositRate;
        //                    var newRequestData = new JObject
        //                    {
        //                        { "BookingId", booking.Id },
        //                        { "Amount", Amount },
        //                        { "AccountId", booking.AccountId },
        //                        { "PodcasterId", booking.PodcastBuddyId },
        //                        { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
        //                    };
        //                    var startSecondSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                        requestData: newRequestData,
        //                        sagaInstanceId: null,
        //                        messageName: "booking-refund-flow");
        //                    await _messagingService.SendSagaMessageAsync(startSecondSagaTriggerMessage, booking.Id.ToString());
        //                    _logger.LogInformation("Booking refund message send successfully for BookingId: {BookingId}", booking.Id);
        //                }
        //            }
        //            await transaction.CommitAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while checking booking deadlines");
        //        }
        //    }
        //}
        //private async Task<JArray?> GetStaffList()
        //{
        //    var batchRequest = new BatchQueryRequest
        //    {
        //        Queries = new List<BatchQueryItem>
        //            {
        //                new BatchQueryItem
        //                {
        //                    Key = "activeStaffList",
        //                    QueryType = "findall",
        //                    EntityType = "Account",
        //                        Parameters = JObject.FromObject(new
        //                        {
        //                            where = new
        //                            {
        //                                IsVerify = true,
        //                                RoleId = (int)RoleEnum.Staff
        //                            },
        //                        }),
        //                }
        //            }
        //    };
        //    var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

        //    var realResult = result.Results?["activeStaffList"] is JArray staffListArray && staffListArray.Count >= 0
        //        ? staffListArray as JArray
        //        : null;
        //    if(realResult != null)
        //    {
        //        for (int i = 0; i < realResult.Count; i++)
        //        {
        //            var staff = realResult[i];
        //            if (staff["Id"] != null && staff["Id"].Type == JTokenType.Integer)
        //            {
        //                // Valid staff Id
        //            }
        //            else
        //            {
        //                // Invalid staff Id, remove this entry
        //                realResult.RemoveAt(i);
        //                i--; // Adjust index after removal
        //            }
        //        }
        //    }
        //}
        private async Task<int> GetRandomStaffFromList(List<AccountDTO>? array)
        {
            //if (array == null || array.Count == 0)
            //    return null;

            //var random = new Random();
            //var randomIndex = random.Next(array.Count);
            //return array[randomIndex];

            var assignedStaffIds = await _bookingGenericRepository.FindAll(
                includeFunc: function => function.Include(b => b.BookingStatusTrackings))
                .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != null &&
                    !new[] {
                        (int)BookingStatusEnum.Completed,
                        (int)BookingStatusEnum.QuotationCancelled,
                        (int)BookingStatusEnum.QuotationRejected,
                        (int)BookingStatusEnum.CancelledManually,
                        (int)BookingStatusEnum.CancelledAutomatically
                    }.Contains(b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId))
                .Select(pbrrs => pbrrs.AssignedStaffId)
                .ToListAsync();

            List<AccountDTO> availableStaff = array;
            Dictionary<int, int> staffAssignmentCount = new Dictionary<int, int>();
            foreach (var staff in availableStaff)
            {
                int count = assignedStaffIds.Count(id => id == staff.Id);
                staffAssignmentCount[staff.Id] = count;
            }

            int minAssignmentCount = staffAssignmentCount.Values.Min();
            List<int> leastAssignedStaffIds = staffAssignmentCount
                .Where(kvp => kvp.Value == minAssignmentCount)
                .Select(kvp => kvp.Key)
                .ToList();
            Random rand = new Random();
            int randomIndex = rand.Next(leastAssignedStaffIds.Count);
            return leastAssignedStaffIds[randomIndex];
        }
        public async Task<bool> ValidateBookingAccountOrPodcasterAsync(int bookingId, int accountId)
        {
            return await ValidateBookingAccountAsync(bookingId, accountId) || await ValidateBookingPodcasterAsync(bookingId, accountId);
        }
        public async Task<bool> ValidateBookingAccountAsync(int bookingId, int accountId)
        {
            var booking = await _bookingGenericRepository.FindByIdAsync(bookingId);
            if (booking == null)
            {
                return false;
            }
            return booking.AccountId == accountId;
        }
        public async Task<bool> ValidateBookingPodcasterAsync(int bookingId, int podcasterId)
        {
            var booking = await _bookingGenericRepository.FindByIdAsync(bookingId);
            if (booking == null)
            {
                return false;
            }
            return booking.PodcastBuddyId == podcasterId;
        }
        //public async Task TerminateBookingOfPodcasterAsync(TerminateBookingOfPodcasterParameterDTO parameter, SagaCommandMessage command)
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var messageName = command.MessageName;
        //            var sagaId = command.SagaInstanceId;
        //            var flowName = command.FlowName;
        //            var responseData = command.LastStepResponseData;

        //            //var booking = _bookingGenericRepository.FindAll()
        //            //    .Include(b => b.BookingStatusTrackings)
        //            //    .Where(b => b.PodcastBuddyId == parameter.PodcasterId && 
        //            //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 3 &&
        //            //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 4 &&
        //            //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 8 &&
        //            //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 9 &&
        //            //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 10 )
        //            //    .ToList();
        //            var booking = _bookingGenericRepository.FindAll()
        //                .Include(b => b.BookingStatusTrackings)
        //                .Where(b => b.PodcastBuddyId == parameter.PodcasterId)
        //                .ToList();
        //            foreach (var b in booking)
        //            {
        //                var currentStatus = b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;
        //                if (currentStatus != (int)BookingStatusEnum.QuotationRejected &&
        //                   currentStatus != (int)BookingStatusEnum.QuotationCancelled &&
        //                   currentStatus != (int)BookingStatusEnum.Completed &&
        //                   currentStatus != (int)BookingStatusEnum.CancelledAutomatically &&
        //                   currentStatus != (int)BookingStatusEnum.CancelledManually)
        //                {
        //                    var newBookingStatusTracking = new BookingStatusTracking
        //                    {
        //                        //Id = Guid.NewGuid(),
        //                        BookingId = b.Id,
        //                        BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
        //                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
        //                    };
        //                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
        //                    b.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //                    b.BookingAutoCancelReason = "TerminatedByPodcaster (bị hủy bởi podcaster)";
        //                    await _bookingGenericRepository.UpdateAsync(b.Id, b);
        //                    if (currentStatus >= (int)BookingStatusEnum.Producing)
        //                    {
        //                        var systemConfig = await GetActiveSystemConfigProfile();
        //                        if(systemConfig == null)
        //                        {
        //                            throw new Exception("System configuration not found");
        //                        }
        //                        var profitRate = systemConfig.BookingConfig.ProfitRate;
        //                        var depositRate = systemConfig.BookingConfig.DepositRate;
        //                        var Amount = b.Price * (decimal)depositRate ;
        //                        var newRequestData = new JObject
        //                        {
        //                            { "BookingId", b.Id },
        //                            { "Amount", Amount },
        //                            { "AccountId", b.AccountId },
        //                            { "PodcasterId", b.PodcastBuddyId },
        //                            { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
        //                        };
        //                        var startSecondSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                            topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                            requestData: newRequestData,
        //                            sagaInstanceId: null,
        //                            messageName: "booking-refund-flow");
        //                        await _messagingService.SendSagaMessageAsync(startSecondSagaTriggerMessage, b.Id.ToString());
        //                        _logger.LogInformation("Booking refund message send successfully for BookingId: {BookingId}", b.Id);
        //                    }
        //                }
        //            }

        //            await transaction.CommitAsync();
        //            var newResponseData = new JObject
        //            {
        //                { "PodcasterId", parameter.PodcasterId },
        //            };
        //            var newMessageName = command.MessageName + ".success";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.BookingManagementDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: command.SagaInstanceId,
        //                flowName: command.FlowName,
        //                messageName: newMessageName);
        //            await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
        //            _logger.LogInformation("Booking terminate success for SagaId: {SagaId}", sagaId.ToString());
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while terminating booking for SagaId: {SagaId}", command.SagaInstanceId);
        //            var newResponseData = new JObject
        //            {
        //                { "ErrorMessage", "Terminate booking failed, error: " + ex.Message }
        //            };
        //            var newMessageName = command.MessageName + ".failed";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.BookingManagementDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: command.SagaInstanceId,
        //                flowName: command.FlowName,
        //                messageName: newMessageName);
        //            await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
        //            _logger.LogError("Booking terminate failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
        //        }
        //    }
        //}
        public async Task CancelPodcasterBookingsTerminatePodcasterForceAsync(CancelPodcasterBookingsTerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var booking = _bookingGenericRepository.FindAll()
                        .Include(b => b.BookingStatusTrackings)
                        .Where(b => b.PodcastBuddyId == parameter.PodcasterId || b.AccountId == parameter.PodcasterId)
                        .ToList();
                    foreach (var b in booking)
                    {
                        var currentStatus = b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;
                        if (currentStatus != (int)BookingStatusEnum.QuotationRejected &&
                           currentStatus != (int)BookingStatusEnum.QuotationCancelled &&
                           currentStatus != (int)BookingStatusEnum.Completed &&
                           currentStatus != (int)BookingStatusEnum.CancelledAutomatically &&
                           currentStatus != (int)BookingStatusEnum.CancelledManually)
                        {
                            var newBookingStatusTracking = new BookingStatusTracking
                            {
                                //Id = Guid.NewGuid(),
                                BookingId = b.Id,
                                BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            };
                            await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                            b.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            if (b.AccountId == parameter.PodcasterId)
                                b.BookingAutoCancelReason = BookingCancelAutoReasonEnum.TerminatePodcasterTaken.GetDescription();
                            else
                                b.BookingAutoCancelReason = BookingCancelAutoReasonEnum.TerminatePodcasterGiven.GetDescription();
                            await _bookingGenericRepository.UpdateAsync(b.Id, b);
                            if (currentStatus >= (int)BookingStatusEnum.Producing)
                            {
                                //// Complete all listen session of lastest producing request
                                //var producingRequest = await _bookingProducingRequestGenericRepository.FindAll(
                                //    includeFunc: function => function
                                //    .Include(pr => pr.BookingPodcastTracks))
                                //    .Where(bpr => bpr.BookingId == b.Id)
                                //    .OrderByDescending(bpr => bpr.CreatedAt)
                                //    .FirstOrDefaultAsync();

                                //var listenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll()
                                //    .Where(ls => !ls.IsCompleted && producingRequest.BookingPodcastTracks.Select(bp => bp.Id).Contains(ls.BookingPodcastTrackId) && ls.AccountId == b.AccountId)
                                //    .ToListAsync();
                                //foreach (var session in listenSession)
                                //{
                                //    session.IsCompleted = true;
                                //    await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                                //}

                                //// Complete all customer listen session procedure of the producing request in cache
                                //var customerListenSessionCacheKey = await _customerListenSessionProcedureCachingService.GetAllProceduresByCustomerIdAsync(b.AccountId);
                                //foreach (var procedure in customerListenSessionCacheKey.Values)
                                //{
                                //    if (!procedure.IsCompleted && procedure.SourceDetail.Booking.BookingProducingRequestId == producingRequest.Id)
                                //    {
                                //        await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(b.AccountId, procedure.Id, true);
                                //    }
                                //}

                                var systemConfig = await GetActiveSystemConfigProfile();
                                if (systemConfig == null)
                                {
                                    throw new Exception("System configuration not found");
                                }
                                var profitRate = systemConfig.BookingConfig.ProfitRate;
                                var depositRate = systemConfig.BookingConfig.DepositRate;

                                if (b.AccountId == parameter.PodcasterId)
                                {
                                    var compensationAmount = b.Price * (decimal)depositRate - b.Price * (decimal)profitRate;
                                    var compensationMessageName = "booking-deposit-compensation-flow";
                                    var compensationRequestData = new JObject
                                    {
                                        { "BookingId", b.Id },
                                        { "Profit", b.Price * (decimal)profitRate },
                                        { "Amount", compensationAmount },
                                        { "AccountId", b.AccountId },
                                        { "PodcasterId", b.PodcastBuddyId },
                                        { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
                                    };
                                    var compensationStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                                        requestData: compensationRequestData,
                                        sagaInstanceId: null,
                                        messageName: compensationMessageName);
                                    await _messagingService.SendSagaMessageAsync(compensationStartSagaTriggerMessage, b.Id.ToString());
                                    _logger.LogInformation("Booking deposit compensation message send successfully for BookingId: {BookingId}", b.Id);
                                }
                                else
                                {
                                    var Amount = b.Price * (decimal)depositRate;
                                    var newRequestData = new JObject
                                    {
                                        { "BookingId", b.Id },
                                        { "Amount", Amount },
                                        { "AccountId", b.AccountId },
                                        { "PodcasterId", b.PodcastBuddyId },
                                        { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
                                    };
                                    var startSecondSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                                        requestData: newRequestData,
                                        sagaInstanceId: null,
                                        messageName: "booking-refund-flow");
                                    await _messagingService.SendSagaMessageAsync(startSecondSagaTriggerMessage, b.Id.ToString());
                                    _logger.LogInformation("Booking refund message send successfully for BookingId: {BookingId}", b.Id);
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "PodcasterId", parameter.PodcasterId },
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Cancel Podcaster Bookings Terminate Podcaster Force successfully for SagaId: {SagaId}", sagaId.ToString());
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Podcaster Bookings Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Cancel Podcaster Bookings Terminate Podcaster Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Cancel Podcaster Bookings Terminate Podcaster Force failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task AddPodcastTonesToPodcasterAsync(AddPodcastTonesToPodcasterParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(parameter.AccountId);
                    if (podcaster == null)
                    {
                        throw new HttpRequestException("Podcaster not found for AccountId: " + parameter.AccountId);
                    }
                    if (!podcaster.HasVerifiedPodcasterProfile)
                    {
                        throw new HttpRequestException("AccountId: " + parameter.AccountId + " is not a verified podcaster");
                    }

                    if ((parameter.PodcastToneIds == null || parameter.PodcastToneIds.Count == 0) && parameter.IsBuddy)
                    {
                        throw new HttpRequestException("PodcastToneIds cannot be null or empty");
                    }

                    var existingBookingPodcastTones = await _podcastBuddyBookingToneGenericRepository.FindAll(
                        predicate: bpt => bpt.PodcasterId == parameter.AccountId
                    ).ToListAsync();

                    await _podcastBuddyBookingToneRepository.DeletePodcastBuddyBookingTone(existingBookingPodcastTones);

                    if (parameter.PodcastToneIds != null || parameter.PodcastToneIds.Count() > 0)
                    {
                        foreach (var podcastToneId in parameter.PodcastToneIds)
                        {
                            var newPodcastBuddyBookingTone = new PodcastBuddyBookingTone
                            {
                                PodcasterId = parameter.AccountId,
                                PodcastBookingToneId = podcastToneId,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            };
                            await _podcastBuddyBookingToneGenericRepository.CreateAsync(newPodcastBuddyBookingTone);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    newResponseData["CreatedAt"] = _dateHelper.GetNowByAppTimeZone();
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("AddPodcastTonesToPodcaster successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while adding podcast tones to podcaster for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Add Podcast Tones To Podcaster failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Add Podcast Tones To Podcaster failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task BookingTrackPreviewingResponseTimeoutAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var systemConfig = await GetActiveSystemConfigProfile();
                    if (systemConfig == null)
                    {
                        throw new Exception("System configuration not found");
                    }
                    var previewResponseAllowedDays = systemConfig.BookingConfig.PreviewResponseAllowedDays;
                    //var previewResponseAllowedDays = 7; 
                    var producingRequestResponseAllowedDays = systemConfig.BookingConfig.ProducingRequestResponseAllowedDays;
                    var profitRate = systemConfig.BookingConfig.ProfitRate;
                    var depositRate = systemConfig.BookingConfig.DepositRate;


                    var currentDateTime = _dateHelper.GetNowByAppTimeZone();
                    var previewingBookingList = _bookingGenericRepository.FindAll()
                        .Include(b => b.BookingProducingRequests)
                        .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.TrackPreviewing &&
                        b.BookingProducingRequests.Where(bpr => bpr.FinishedAt != null).OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt.HasValue &&
                        b.BookingProducingRequests.Where(bpr => bpr.FinishedAt != null).OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt!.Value.AddDays(previewResponseAllowedDays) < currentDateTime)
                        .ToList();

                    foreach (var booking in previewingBookingList)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            //Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingAutoCancelReason = BookingCancelAutoReasonEnum.ExpiredPreview.GetDescription();
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                        if (booking.Price.HasValue)
                        {
                            var Amount = booking.Price * (decimal)depositRate - booking.Price * (decimal)profitRate;
                            var compensationMessageName = "booking-deposit-compensation-flow";
                            var newRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
                                { "Profit", booking.Price * (decimal)profitRate },
                                { "Amount", Amount },
                                { "AccountId", booking.AccountId },
                                { "PodcasterId", booking.PodcastBuddyId },
                                { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
                            };
                            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: newRequestData,
                                sagaInstanceId: null,
                                messageName: compensationMessageName);
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, booking.Id.ToString());
                            _logger.LogInformation("Booking deposit compensation message send successfully for BookingId: {BookingId}", booking.Id);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while checking booking track previewing responses timeout, error: " + ex.Message);
                }
            }
        }
        public async Task BookingProducingRequestedResponseTimeoutAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var systemConfig = await GetActiveSystemConfigProfile();
                    if (systemConfig == null)
                    {
                        throw new Exception("System configuration not found");
                    }
                    var previewResponseAllowedDays = systemConfig.BookingConfig.PreviewResponseAllowedDays;
                    var producingRequestResponseAllowedDays = systemConfig.BookingConfig.ProducingRequestResponseAllowedDays;
                    var profitRate = systemConfig.BookingConfig.ProfitRate;
                    var depositRate = systemConfig.BookingConfig.DepositRate;


                    var currentDateTime = _dateHelper.GetNowByAppTimeZone();

                    var producingRequestBookingList = _bookingGenericRepository.FindAll()
                        .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.ProducingRequested &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().IsAccepted == null &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().CreatedAt.AddDays(producingRequestResponseAllowedDays) < currentDateTime)
                        .Include(b => b.BookingProducingRequests)
                        .ToList();
                    foreach (var booking in producingRequestBookingList)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            //Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingAutoCancelReason = BookingCancelAutoReasonEnum.PodcastBuddyNoResponse.GetDescription();
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                        var startFirstSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.UserManagementDomain,
                            requestData: new JObject
                            {
                                { "AccountId", booking.PodcastBuddyId },
                                { "ViolationPoint", 1 },
                            },
                            sagaInstanceId: null,
                            messageName: "user-violation-punishment-flow");
                        await _messagingService.SendSagaMessageAsync(startFirstSagaTriggerMessage, booking.Id.ToString());
                        _logger.LogInformation("User violation punishment message send successfully to AccountId: {AccountId} for BookingId: {BookingId}", booking.PodcastBuddyId, booking.Id);

                        if (booking.Price.HasValue)
                        {
                            var Amount = booking.Price * (decimal)depositRate;
                            var newRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
                                { "Amount", Amount },
                                { "AccountId", booking.AccountId },
                                { "PodcasterId", booking.PodcastBuddyId },
                                { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
                            };
                            var startSecondSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: newRequestData,
                                sagaInstanceId: null,
                                messageName: "booking-refund-flow");
                            await _messagingService.SendSagaMessageAsync(startSecondSagaTriggerMessage, booking.Id.ToString());
                            _logger.LogInformation("Booking refund message send successfully for BookingId: {BookingId}", booking.Id);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while checking booking producing requested response timeout, error: " + ex.Message);
                }
            }
        }
        public async Task<List<BookingCompletedListItemResponseDTO>> GetCompletedBookingsByAccountIdAsync(int accountId, bool isPodcaster)
        {
            try
            {
                var query = _bookingGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(b => b.BookingStatusTrackings)
                    .ThenInclude(bst => bst.BookingStatus)
                    .Include(b => b.BookingProducingRequests)
                    .ThenInclude(bpr => bpr.BookingPodcastTracks));
                if (isPodcaster)
                {
                    query = query.Where(b => b.PodcastBuddyId == accountId);
                }
                else
                {
                    query = query.Where(b => b.AccountId == accountId);
                }

                query = query.Where(b => b.BookingStatusTrackings
                    .OrderByDescending(bst => bst.CreatedAt)
                    .FirstOrDefault()
                    .BookingStatusId == (int)BookingStatusEnum.Completed);
                var bookings = await query.ToListAsync();

                var result = new List<BookingCompletedListItemResponseDTO>();

                foreach (var booking in bookings)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(booking.PodcastBuddyId);
                    AccountStatusCache? assignedStaff = null;
                    if (booking.AssignedStaffId.HasValue)
                    {
                        assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                    }
                    result.Add(new BookingCompletedListItemResponseDTO
                    {
                        Id = booking.Id,
                        Title = booking.Title,
                        Description = booking.Description,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastBuddy = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                        {
                            Id = assignedStaff.Id,
                            FullName = assignedStaff.FullName,
                            Email = assignedStaff.Email,
                            MainImageFileKey = assignedStaff.MainImageFileKey
                        } : null,
                        Price = booking.Price,
                        Deadline = booking.Deadline,
                        DeadlineDays = booking.DeadlineDays,
                        DemoAudioFileKey = booking.DemoAudioFileKey,
                        BookingManualCancelledReason = booking.BookingManualCancelledReason,
                        BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                        CompletedBookingTrackCount = booking.BookingProducingRequests
                            .Where(bpr => bpr.FinishedAt != null)
                            .OrderByDescending(bpr => bpr.CreatedAt)
                            .FirstOrDefault()
                            .BookingPodcastTracks.Count(),
                        CompletedAt = booking.BookingStatusTrackings
                            .Where(bst => bst.BookingStatusId == (int)BookingStatusEnum.Completed)
                            .OrderByDescending(bst => bst.CreatedAt)
                            .FirstOrDefault()
                            .CreatedAt,
                        CreatedAt = booking.CreatedAt,
                        UpdatedAt = booking.UpdatedAt,
                        CurrentStatus = new BookingStatusResponseDTO
                        {
                            Id = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Id,
                            Name = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Name
                        }
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting completed bookings for AccountId: {AccountId}", accountId);
                throw new HttpRequestException("Error occurred while retrieving completed booking, error: " + ex.Message);
            }
        }
        public async Task<BookingDetailResponseDTO> GetCompletedBookingDetailByAccountIdAsync(int accountId, int bookingId)
        {
            try
            {
                var booking = await _bookingGenericRepository.FindByIdAsync(
                    bookingId,
                    includeFunc: function => function
                    .Include(b => b.BookingRequirements)
                        .ThenInclude(br => br.PodcastBookingTone)
                        .ThenInclude(bt => bt.PodcastBookingToneCategory)
                    .Include(b => b.BookingProducingRequests)
                        .ThenInclude(bp => bp.BookingPodcastTracks)
                            .ThenInclude(bpt => bpt.BookingRequirement)
                    .Include(b => b.BookingStatusTrackings)
                        .ThenInclude(bst => bst.BookingStatus));

                if (booking == null)
                    return null;

                if (booking.BookingStatusTrackings
                    .OrderByDescending(bst => bst.CreatedAt)
                    .FirstOrDefault()
                    .BookingStatusId != (int)BookingStatusEnum.Completed)
                {
                    throw new HttpRequestException("Booking is not completed yet");
                }

                if (booking.AccountId != accountId)
                {
                    throw new HttpRequestException("Account not authorize to view the detail of this booking");
                }

                var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                var podcastBuddy = await GetPodcaster(booking.PodcastBuddyId);
                AccountStatusCache? assignedStaff = null;
                if (booking.AssignedStaffId != null)
                {
                    assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                }

                if (podcastBuddy == null)
                {
                    throw new HttpRequestException("PodcastBuddy not found");
                }

                var latestStatus = booking.BookingStatusTrackings?
                    .OrderByDescending(bst => bst.CreatedAt)
                    .FirstOrDefault();

                var lastestProducingRequest = booking.BookingProducingRequests?.Where(bp => bp.FinishedAt != null).OrderByDescending(bp => bp.CreatedAt).FirstOrDefault();

                var lastestBookingPodcastTracks = new List<BookingPodcastTrackListItemResponseDTO>();
                if (lastestProducingRequest != null)
                {
                    lastestBookingPodcastTracks = lastestProducingRequest.BookingPodcastTracks.OrderBy(bpt => bpt.BookingRequirement.Order).Select(bpt => new BookingPodcastTrackListItemResponseDTO
                    {
                        Id = bpt.Id,
                        BookingId = bpt.BookingId,
                        BookingRequirementId = bpt.BookingRequirementId,
                        BookingProducingRequestId = bpt.BookingProducingRequestId,
                        AudioFileKey = bpt.AudioFileKey,
                        AudioFileSize = bpt.AudioFileSize,
                        AudioLength = bpt.AudioLength,
                        RemainingPreviewListenSlot = bpt.RemainingPreviewListenSlot
                    }).ToList();
                }

                return new BookingDetailResponseDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    Account = new AccountSnippetResponseDTO
                    {
                        Id = account.Id,
                        FullName = account.FullName,
                        Email = account.Email,
                        MainImageFileKey = account.MainImageFileKey
                    },
                    PodcastBuddy = new PodcastBuddySnippetResponseDTO
                    {
                        Id = podcastBuddy.Id,
                        FullName = podcastBuddy.PodcasterProfile.Name,
                        Email = podcastBuddy.Email,
                        Description = podcastBuddy.PodcasterProfile?.Description ?? string.Empty,
                        MainImageFileKey = podcastBuddy.MainImageFileKey,
                        AverageRating = podcastBuddy.PodcasterProfile?.AverageRating ?? 0,
                        RatingCount = podcastBuddy.PodcasterProfile?.RatingCount ?? 0,
                        TotalFollow = podcastBuddy.PodcasterProfile?.TotalFollow ?? 0,
                        TotalBookingCompleted = 0,
                        PriceBookingPerWord = podcastBuddy.PodcasterProfile?.PricePerBookingWord ?? 0
                    },
                    AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                    {
                        Id = assignedStaff.Id,
                        FullName = assignedStaff.FullName,
                        Email = assignedStaff.Email,
                        MainImageFileKey = assignedStaff.MainImageFileKey
                    } : null,
                    Price = booking.Price,
                    Deadline = booking.Deadline,
                    DeadlineDays = booking.DeadlineDays,
                    DemoAudioFileKey = booking.DemoAudioFileKey,
                    BookingManualCancelledReason = booking.BookingManualCancelledReason,
                    BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                    CreatedAt = booking.CreatedAt,
                    UpdatedAt = booking.UpdatedAt,
                    BookingRequirementFileList = booking.BookingRequirements?.Select(re => new BookingRequirementListItemResponseDTO
                    {
                        Id = re.Id,
                        BookingId = re.BookingId,
                        Name = re.Name,
                        Description = re.Description,
                        RequirementDocumentFileKey = re.RequirementDocumentFileKey,
                        Order = re.Order,
                        WordCount = re.WordCount,
                        PodcastBookingTone = new PodcastBookingToneDetailResponseDTO()
                        {
                            Id = re.PodcastBookingTone.Id,
                            Name = re.PodcastBookingTone.Name,
                            Description = re.PodcastBookingTone.Description ?? string.Empty,
                            CreatedAt = re.PodcastBookingTone.CreatedAt,
                            DeletedAt = re.PodcastBookingTone.DeletedAt,
                            PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO()
                            {
                                Id = re.PodcastBookingTone.PodcastBookingToneCategory.Id,
                                Name = re.PodcastBookingTone.PodcastBookingToneCategory.Name
                            }
                        }
                    }).ToList() ?? new List<BookingRequirementListItemResponseDTO>(),
                    BookingProducingRequestList = booking.BookingProducingRequests?.OrderByDescending(bp => bp.CreatedAt).Select(prod => new BookingProducingRequestListItemResponseDTO
                    {
                        Id = prod.Id,
                        BookingId = prod.BookingId,
                        Note = prod.Note,
                        Deadline = prod.Deadline,
                        DeadlineDays = prod.DeadlineDays,
                        RejectReason = prod.RejectReason,
                        IsAccepted = prod.IsAccepted,
                        FinishedAt = prod.FinishedAt,
                        CreatedAt = prod.CreatedAt
                    }).ToList() ?? new List<BookingProducingRequestListItemResponseDTO>(),
                    CurrentStatus = new BookingStatusResponseDTO
                    {
                        Id = latestStatus?.BookingStatus?.Id ?? 0,
                        Name = latestStatus?.BookingStatus?.Name ?? "Unknown"
                    },
                    StatusTracking = booking.BookingStatusTrackings?.Select(bst => new BookingStatusTrackingListItemResponseDTO
                    {
                        Id = bst.Id,
                        BookingId = bst.BookingId,
                        BookingStatusId = bst.BookingStatusId,
                        CreatedAt = bst.CreatedAt
                    }).OrderByDescending(bst => bst.CreatedAt).ToList() ?? new List<BookingStatusTrackingListItemResponseDTO>(),
                    LastestBookingPodcastTracks = lastestBookingPodcastTracks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking with ID: {BookingId}", bookingId);
                throw new HttpRequestException($"Retrieving Booking failed. Error: {ex.Message}");
            }
        }
        public async Task<List<PodcastBuddySnippetResponseDTO>> GetPodcastersByBookingToneIdAsync(Guid podcastBookingToneId)
        {
            try
            {
                var result = new List<PodcastBuddySnippetResponseDTO>();
                var podcastBuddyBookingTones = await _podcastBuddyBookingToneGenericRepository.FindAll(
                    predicate: pbbt => pbbt.PodcastBookingToneId == podcastBookingToneId
                    )
                    .Select(pbbt => pbbt.PodcasterId)
                    .Distinct()
                    .ToListAsync();
                foreach (var podcasterId in podcastBuddyBookingTones)
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(podcasterId);
                    var podcastReal = await GetPodcaster(podcasterId);
                    if (podcaster != null && podcaster.HasVerifiedPodcasterProfile && podcaster.PodcasterProfileIsBuddy && podcaster.IsVerified)
                    {
                        result.Add(new PodcastBuddySnippetResponseDTO
                        {
                            Id = podcastReal.Id,
                            FullName = podcastReal.PodcasterProfile.Name,
                            Email = podcastReal.Email,
                            Description = podcastReal.PodcasterProfile?.Description ?? string.Empty,
                            MainImageFileKey = podcastReal.MainImageFileKey,
                            AverageRating = podcastReal.PodcasterProfile?.AverageRating ?? 0,
                            RatingCount = podcastReal.PodcasterProfile?.RatingCount ?? 0,
                            TotalFollow = podcastReal.PodcasterProfile?.TotalFollow ?? 0,
                            TotalBookingCompleted = _bookingGenericRepository.FindAll(
                                includeFunc: function => function
                                .Include(b => b.BookingStatusTrackings))
                            .Where(b => b.PodcastBuddyId == podcastReal.Id && b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId == (int)BookingStatusEnum.Completed)
                            .Count(),
                            PriceBookingPerWord = podcastReal.PodcasterProfile?.PricePerBookingWord ?? 0
                        });
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting podcasters for PodcastBookingToneId: {PodcastBookingToneId}", podcastBookingToneId);
                throw new HttpRequestException("Error occurred while retrieving podcasters by booking tone, error: " + ex.Message);
            }
        }
        public async Task<BookingResultDetailResponseDTO> GetBookingResultByIdAsync(int bookingId, int accountId)
        {
            try
            {
                var booking = await _bookingGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(b => b.BookingStatusTrackings)
                    .ThenInclude(bst => bst.BookingStatus)
                    .Include(b => b.BookingProducingRequests)
                        .ThenInclude(bpr => bpr.BookingPodcastTracks))
                    .Where(b => b.Id == bookingId)
                    .FirstOrDefaultAsync();
                if (booking == null)
                {
                    throw new HttpRequestException("Booking not found for BookingId: " + bookingId);
                }
                if (booking.AccountId != accountId && booking.PodcastBuddyId != accountId)
                {
                    throw new HttpRequestException("You are not authorized to view this booking");
                }
                if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != (int)BookingStatusEnum.Completed)
                {
                    throw new HttpRequestException("Booking is not completed yet for BookingId: " + bookingId);
                }

                var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                var podcaster = await GetPodcaster(booking.PodcastBuddyId);
                if (podcaster == null)
                {
                    throw new HttpRequestException($"Podcaster with ID {booking.PodcastBuddyId} not found");
                }
                AccountStatusCache? assignedStaff = null;
                if (booking.AssignedStaffId.HasValue)
                {
                    assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                }
                return new BookingResultDetailResponseDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    Account = new AccountSnippetResponseDTO
                    {
                        Id = account.Id,
                        FullName = account.FullName,
                        Email = account.Email,
                        MainImageFileKey = account.MainImageFileKey
                    },
                    PodcastBuddy = new PodcastBuddySnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        FullName = podcaster.PodcasterProfile.Name,
                        Email = podcaster.Email,
                        Description = podcaster.PodcasterProfile?.Description ?? string.Empty,
                        MainImageFileKey = podcaster.MainImageFileKey,
                        AverageRating = podcaster.PodcasterProfile?.AverageRating ?? 0,
                        RatingCount = podcaster.PodcasterProfile?.RatingCount ?? 0,
                        TotalFollow = podcaster.PodcasterProfile?.TotalFollow ?? 0,
                        TotalBookingCompleted = 0,
                        PriceBookingPerWord = podcaster.PodcasterProfile?.PricePerBookingWord
                    },
                    AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                    {
                        Id = assignedStaff.Id,
                        FullName = assignedStaff.FullName,
                        Email = assignedStaff.Email,
                        MainImageFileKey = assignedStaff.MainImageFileKey
                    } : null,
                    Deadline = booking.Deadline,
                    DeadlineDays = booking.DeadlineDays,
                    Price = booking.Price,
                    DemoAudioFileKey = booking.DemoAudioFileKey,
                    BookingManualCancelledReason = booking.BookingManualCancelledReason,
                    BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                    CreatedAt = booking.CreatedAt,
                    UpdatedAt = booking.UpdatedAt,
                    CurrentStatus = new BookingStatusResponseDTO
                    {
                        Id = booking.BookingStatusTrackings
                            .OrderByDescending(bst => bst.CreatedAt)
                            .FirstOrDefault().BookingStatus.Id,
                        Name = booking.BookingStatusTrackings
                            .OrderByDescending(bst => bst.CreatedAt)
                            .FirstOrDefault().BookingStatus.Name
                    },
                    BookingRequirementFileList = booking.BookingRequirements
                        .OrderBy(brf => brf.Order)
                        .Select(brf => new BookingRequirementListItemResponseDTO
                        {
                            Id = brf.Id,
                            BookingId = brf.BookingId,
                            Description = brf.Description,
                            Name = brf.Name,
                            Order = brf.Order,
                            PodcastBookingTone = new PodcastBookingToneDetailResponseDTO
                            {
                                Id = brf.PodcastBookingTone.Id,
                                Name = brf.PodcastBookingTone.Name,
                                Description = brf.PodcastBookingTone.Description,
                            },
                            RequirementDocumentFileKey = brf.RequirementDocumentFileKey,
                            WordCount = brf.WordCount
                        })
                        .ToList(),
                    BookingPodcastTrackList = booking.BookingProducingRequests.OrderByDescending(bp => bp.CreatedAt)
                        .FirstOrDefault()
                        .BookingPodcastTracks
                        .Select(bpt => new BookingPodcastTrackListItemResponseDTO
                        {
                            Id = bpt.Id,
                            BookingId = bpt.BookingId,
                            BookingRequirementId = bpt.BookingRequirementId,
                            BookingProducingRequestId = bpt.BookingProducingRequestId,
                            AudioFileKey = bpt.AudioFileKey,
                            AudioFileSize = bpt.AudioFileSize,
                            AudioLength = bpt.AudioLength,
                            RemainingPreviewListenSlot = bpt.RemainingPreviewListenSlot
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking result for BookingId: {BookingId}", bookingId);
                throw new HttpRequestException("Error occurred while retrieving booking result, error: " + ex.Message);
            }
        }
        public async Task<List<BookingIncomeStatisticReportListItemResponseDTO>> GetBookingIncomeStatisticAsync(StatisticsReportPeriodEnum statisticEnum, AccountStatusCache account)
        {
            try
            {
                List<BookingIncomeStatisticReportListItemResponseDTO> statisticList = new List<BookingIncomeStatisticReportListItemResponseDTO>();
                if (statisticEnum == StatisticsReportPeriodEnum.Daily)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    // truy về 7 ngày gần nhất, bao gồm hôm nay, trả về mảng List<ProfitPeriodicReportListItemDTO>
                    DateOnly startDate = today.AddDays(-6);
                    for (int i = 0; i < 7; i++)
                    {
                        DateOnly currentDate = startDate.AddDays(i);
                        var amount = await CalculateIncome(account.Id, currentDate, currentDate);

                        statisticList.Add(new BookingIncomeStatisticReportListItemResponseDTO
                        {
                            StartDate = currentDate,
                            EndDate = currentDate,
                            Amount = amount
                        });
                    }

                    return statisticList;
                }
                else if (statisticEnum == StatisticsReportPeriodEnum.Monthly)
                {
                    // truy vấn về 4 tuần trong tháng
                    DateOnly startDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetLastDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly currentStartDate = startDateOfMonth;
                    while (currentStartDate <= endDateOfMonth)
                    {
                        DateOnly currentEndDate = currentStartDate.AddDays(6);
                        if (currentEndDate > endDateOfMonth)
                        {
                            currentEndDate = endDateOfMonth;
                        }

                        var amount = await CalculateIncome(account.Id, currentStartDate, currentEndDate);
                        statisticList.Add(new BookingIncomeStatisticReportListItemResponseDTO
                        {
                            StartDate = currentStartDate,
                            EndDate = currentEndDate,
                            Amount = amount
                        });
                        currentStartDate = currentEndDate.AddDays(1);
                    }
                    return statisticList;
                }
                else if (statisticEnum == StatisticsReportPeriodEnum.Yearly)
                {
                    // truy vấn về 12 tháng trong năm
                    DateOnly startDateOfYear = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfYear = DateOnly.FromDateTime(_dateHelper.GetLastDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly currentStartDate = startDateOfYear;
                    while (currentStartDate <= endDateOfYear)
                    {
                        DateOnly currentEndDate = currentStartDate.AddMonths(1).AddDays(-1);
                        if (currentEndDate > endDateOfYear)
                        {
                            currentEndDate = endDateOfYear;
                        }

                        var amount = await CalculateIncome(account.Id, currentStartDate, currentEndDate);
                        statisticList.Add(new BookingIncomeStatisticReportListItemResponseDTO
                        {
                            StartDate = currentStartDate,
                            EndDate = currentEndDate,
                            Amount = amount
                        });
                        currentStartDate = currentEndDate.AddDays(1);
                    }
                    return statisticList;
                }
                else
                {
                    throw new HttpRequestException("Selected Period Enum is not Supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking income statistic report for Period: {Period}", statisticEnum.ToString());
                throw new HttpRequestException("Error occurred while retrieving booking income statistic report, error: " + ex.Message);
            }
        }
        public async Task<List<BookingIncomeStatisticReportListItemResponseDTO>> GetSystemBookingIncomeStatisticAsync(StatisticsReportPeriodEnum statisticEnum)
        {
            try
            {

                List<BookingIncomeStatisticReportListItemResponseDTO> statisticList = new List<BookingIncomeStatisticReportListItemResponseDTO>();
                if (statisticEnum == StatisticsReportPeriodEnum.Daily)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    // truy về 7 ngày gần nhất, bao gồm hôm nay, trả về mảng List<ProfitPeriodicReportListItemDTO>
                    DateOnly startDate = today.AddDays(-6);
                    for (int i = 0; i < 7; i++)
                    {
                        DateOnly currentDate = startDate.AddDays(i);
                        var amount = await CalculateSystemIncome(currentDate, currentDate);

                        statisticList.Add(new BookingIncomeStatisticReportListItemResponseDTO
                        {
                            StartDate = currentDate,
                            EndDate = currentDate,
                            Amount = amount
                        });
                    }

                    return statisticList;
                }
                else if (statisticEnum == StatisticsReportPeriodEnum.Monthly)
                {
                    // truy vấn về 4 tuần trong tháng
                    DateOnly startDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetLastDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly currentStartDate = startDateOfMonth;
                    while (currentStartDate <= endDateOfMonth)
                    {
                        DateOnly currentEndDate = currentStartDate.AddDays(6);
                        if (currentEndDate > endDateOfMonth)
                        {
                            currentEndDate = endDateOfMonth;
                        }

                        var amount = await CalculateSystemIncome(currentStartDate, currentEndDate);
                        statisticList.Add(new BookingIncomeStatisticReportListItemResponseDTO
                        {
                            StartDate = currentStartDate,
                            EndDate = currentEndDate,
                            Amount = amount
                        });
                        currentStartDate = currentEndDate.AddDays(1);
                    }
                    return statisticList;
                }
                else if (statisticEnum == StatisticsReportPeriodEnum.Yearly)
                {
                    // truy vấn về 12 tháng trong năm
                    DateOnly startDateOfYear = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfYear = DateOnly.FromDateTime(_dateHelper.GetLastDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly currentStartDate = startDateOfYear;
                    while (currentStartDate <= endDateOfYear)
                    {
                        DateOnly currentEndDate = currentStartDate.AddMonths(1).AddDays(-1);
                        if (currentEndDate > endDateOfYear)
                        {
                            currentEndDate = endDateOfYear;
                        }

                        var amount = await CalculateSystemIncome(currentStartDate, currentEndDate);
                        statisticList.Add(new BookingIncomeStatisticReportListItemResponseDTO
                        {
                            StartDate = currentStartDate,
                            EndDate = currentEndDate,
                            Amount = amount
                        });
                        currentStartDate = currentEndDate.AddDays(1);
                    }
                    return statisticList;
                }
                else
                {
                    throw new HttpRequestException("Selected Period Enum is not Supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking income statistic report for Period: {Period}", statisticEnum.ToString());
                throw new HttpRequestException("Error occurred while retrieving booking income statistic report, error: " + ex.Message);
            }
        }
        public async Task<BookingIncomeTotalStatisticReportResponseDTO> GetTotalSystemBookingIncomeStatisticAsync(StatisticsReportPeriodEnum statisticEnum)
        {
            try
            {
                BookingIncomeTotalStatisticReportResponseDTO statisticResult = new BookingIncomeTotalStatisticReportResponseDTO();
                if (statisticEnum == StatisticsReportPeriodEnum.Daily)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    DateOnly previousDay = today.AddDays(-1);
                    var currentTotalAmount = await CalculateSystemIncome(today, today);
                    var previousTotalAmount = await CalculateSystemIncome(previousDay, previousDay);

                    statisticResult = new BookingIncomeTotalStatisticReportResponseDTO
                    {
                        TotalBookingIncomeAmount = currentTotalAmount,
                        TotalBookingIncomePercentChange = previousTotalAmount == 0 ? 100 : Math.Round(((double)(currentTotalAmount - previousTotalAmount) / (double)previousTotalAmount * 100), 2, MidpointRounding.AwayFromZero),
                    };

                    return statisticResult;
                }
                else if (statisticEnum == StatisticsReportPeriodEnum.Monthly)
                {
                    DateOnly startDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetLastDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    var previousStartDateOfMonth = startDateOfMonth.AddMonths(-1);
                    var previousEndDateOfMonth = endDateOfMonth.AddMonths(-1);

                    var currentTotalAmount = await CalculateSystemIncome(startDateOfMonth, endDateOfMonth);
                    var previousTotalAmount = await CalculateSystemIncome(previousStartDateOfMonth, previousEndDateOfMonth);

                    statisticResult = new BookingIncomeTotalStatisticReportResponseDTO
                    {
                        TotalBookingIncomeAmount = currentTotalAmount,
                        TotalBookingIncomePercentChange = previousTotalAmount == 0 ? 100 : Math.Round(((double)(currentTotalAmount - previousTotalAmount) / (double)previousTotalAmount * 100), 2, MidpointRounding.AwayFromZero),
                    };

                    return statisticResult;
                }
                else if (statisticEnum == StatisticsReportPeriodEnum.Yearly)
                {
                    DateOnly startDateOfYear = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfYear = DateOnly.FromDateTime(_dateHelper.GetLastDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    var previousStartDateOfYear = startDateOfYear.AddYears(-1);
                    var previousEndDateOfYear = endDateOfYear.AddYears(-1);

                    var currentTotalAmount = await CalculateSystemIncome(startDateOfYear, endDateOfYear);
                    var previousTotalAmount = await CalculateSystemIncome(previousStartDateOfYear, previousEndDateOfYear);

                    statisticResult = new BookingIncomeTotalStatisticReportResponseDTO
                    {
                        TotalBookingIncomeAmount = currentTotalAmount,
                        TotalBookingIncomePercentChange = previousTotalAmount == 0 ? 100 : Math.Round(((double)(currentTotalAmount - previousTotalAmount) / (double)previousTotalAmount * 100), 2, MidpointRounding.AwayFromZero),
                    };

                    return statisticResult;
                }
                else
                {
                    throw new HttpRequestException("Selected Period Enum is not Supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting total system booking income statistic report for Period: {Period}", statisticEnum.ToString());
                throw new HttpRequestException("Error occurred while retrieving total system booking income statistic report, error: " + ex.Message);
            }
        }
        private async Task<decimal> CalculateSystemIncome(DateOnly startDate, DateOnly endDate)
        {
            decimal totalIncome = 0;
            var bookingList = await _bookingGenericRepository.FindAll(
                includeFunc: function => function
                .Include(b => b.BookingStatusTrackings))
                .Where(b => (b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.Completed ||
                b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.CancelledManually ||
                b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.CancelledAutomatically)
                && (startDate <= DateOnly.FromDateTime(b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().CreatedAt) &&
                DateOnly.FromDateTime(b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().CreatedAt) <= endDate)).ToListAsync();
            foreach (var booking in bookingList)
            {
                Console.WriteLine("Booking ID: " + booking.Id);
                var bookingTransactions = await GetSystemBookingTransactionByBookingId(booking.Id);
                totalIncome += bookingTransactions != null ? bookingTransactions.Sum(bt => bt.Amount) : 0;
                Console.WriteLine("Total Income so far: " + totalIncome);
            }
            return totalIncome;
        }
        private async Task<decimal> CalculateIncome(int accountId, DateOnly startDate, DateOnly endDate)
        {
            decimal totalIncome = 0;
            var bookingList = await _bookingGenericRepository.FindAll(
                includeFunc: function => function
                .Include(b => b.BookingStatusTrackings))
                .Where(b => (b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.Completed ||
                b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.CancelledManually ||
                b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.CancelledAutomatically)
                && (startDate <= DateOnly.FromDateTime(b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().CreatedAt) &&
                DateOnly.FromDateTime(b.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).First().CreatedAt) <= endDate)
                && b.PodcastBuddyId == accountId).ToListAsync();
            foreach (var booking in bookingList)
            {
                Console.WriteLine("Booking ID: " + booking.Id);
                var bookingTransactions = await GetBookingTransactionByBookingId(booking.Id);
                totalIncome += bookingTransactions != null ? bookingTransactions.Sum(bt => bt.Amount) : 0;
                Console.WriteLine("Total Income so far: " + totalIncome);
            }
            return totalIncome;
        }
        private async Task<PodcasterDTO?> GetPodcaster(int accountId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcaster",
                            QueryType = "findall",
                            EntityType = "Account",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = accountId
                                },
                                include = "PodcasterProfile"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

                return result.Results?["podcaster"] is JArray podcasterArray && podcasterArray.Count > 0
                   ? podcasterArray.First.ToObject<PodcasterDTO>()
                   : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while querying podcaster for AccountId: {AccountId}", accountId);
                throw new HttpRequestException("Error occurred while querying podcaster, error: " + ex.Message);
            }
        }
        private async Task<SystemConfigProfileDTO?> GetActiveSystemConfigProfile()
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeSystemConfigProfile",
                            QueryType = "findall",
                            EntityType = "SystemConfigProfile",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsActive = true
                                    },
                                    include = "AccountConfig, AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

                return result.Results?["activeSystemConfigProfile"] is JArray configArray && configArray.Count > 0
                    ? configArray.First.ToObject<SystemConfigProfileDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while querying active system config profile");
                throw new HttpRequestException("Error occurred while querying active system config profile, error: " + ex.Message);
            }
        }
        private async Task<List<BookingTransactionDTO>?> GetBookingTransactionByBookingId(int bookingId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcasterIncomeTransactions",
                            QueryType = "findall",
                            EntityType = "BookingTransaction",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        BookingId = bookingId,
                                        TransactionTypeId = (int)TransactionTypeEnum.PodcasterBookingIncome,
                                        TransactionStatusId = (int)TransactionStatusEnum.Success
                                    }
                                })
                        },
                        // Query 2: BookingDepositCompensation
                        new BatchQueryItem
                        {
                            Key = "compensationTransactions",
                            QueryType = "findall",
                            EntityType = "BookingTransaction",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    BookingId = bookingId,
                                    TransactionTypeId = (int)TransactionTypeEnum.BookingDepositCompensation,
                                    TransactionStatusId = (int)TransactionStatusEnum.Success
                                }
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("TransactionService", batchRequest);

                // Combine results from both queries
                var combinedTransactions = new List<BookingTransactionDTO>();

                // Add results from first query
                if (result.Results?["podcasterIncomeTransactions"] is JArray incomeArray)
                {
                    var incomeTransactions = incomeArray.ToObject<List<BookingTransactionDTO>>();
                    if (incomeTransactions != null)
                        combinedTransactions.AddRange(incomeTransactions);
                }

                // Add results from second query
                if (result.Results?["compensationTransactions"] is JArray compensationArray)
                {
                    var compensationTransactions = compensationArray.ToObject<List<BookingTransactionDTO>>();
                    if (compensationTransactions != null)
                        combinedTransactions.AddRange(compensationTransactions);
                }

                return combinedTransactions.Count > 0 ? combinedTransactions : null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while querying booking transaction");
                throw new HttpRequestException("Error occurred while querying booking transaction, error: " + ex.Message);
            }
        }
        private async Task<BookingTransactionDTO?> GetBookingHoldingTransactionByBookingId(int bookingId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "bookingTransaction",
                            QueryType = "findall",
                            EntityType = "BookingTransaction",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    BookingId = bookingId,
                                    TransactionTypeId = (int)TransactionTypeEnum.BookingDeposit,
                                    TransactionStatusId = (int)TransactionStatusEnum.Success
                                }

                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("TransactionService", batchRequest);

                return result.Results?["bookingTransaction"] is JArray bookingArray && bookingArray.Count > 0
                    ? bookingArray.First.ToObject<BookingTransactionDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while querying booking transaction");
                throw new HttpRequestException("Error occurred while querying booking transaction, error: " + ex.Message);
            }
        }
        private async Task<List<BookingTransactionDTO>?> GetSystemBookingTransactionByBookingId(int bookingId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "bookingTransaction",
                            QueryType = "findall",
                            EntityType = "BookingTransaction",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    BookingId = bookingId,
                                    TransactionTypeId = (int)TransactionTypeEnum.SystemBookingIncome,
                                    TransactionStatusId = (int)TransactionStatusEnum.Success
                                }

                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("TransactionService", batchRequest);

                return result.Results?["bookingTransaction"] is JArray bookingArray && bookingArray.Count >= 0
                    ? bookingArray.ToObject<List<BookingTransactionDTO>>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while querying booking transaction");
                throw new HttpRequestException("Error occurred while querying booking transaction, error: " + ex.Message);
            }
        }
        public async Task<BookingListenSessionResponseDTO> GetTrackListenAsync(int bookingId, Guid podcastTrackId, int accountId, DeviceInfoDTO deviceInfo, CustomerListenSessionProcedureSourceDetailTypeEnum sourceType)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var booking = await _bookingGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings)
                        .Include(b => b.BookingProducingRequests)
                        .ThenInclude(bp => bp.BookingPodcastTracks)
                        .ThenInclude(bpt => bpt.BookingRequirement))
                        .Where(b => b.Id == bookingId)
                        .FirstOrDefaultAsync();

                    if (booking == null)
                    {
                        throw new HttpRequestException("Booking with id " + bookingId + " does not exist");
                    }
                    if (booking.AccountId != accountId && booking.PodcastBuddyId != accountId)
                    {
                        throw new HttpRequestException("You are not authorized to listen to tracks of this booking");
                    }
                    var currentBookingStatusId = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault()
                        .BookingStatusId;

                    var currentBookingProducingRequest = booking.BookingProducingRequests
                        .Where(bpr => bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();

                    var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.BookingId == bookingId && bpt.Id == podcastTrackId,
                        includeFunc: bpt => bpt.Include(b => b.BookingRequirement)
                    ).FirstOrDefaultAsync();

                    if (bookingPodcastTrack == null)
                    {
                        throw new HttpRequestException("Podcast track with id " + podcastTrackId + " does not exist");
                    }

                    var check = await CheckListenerCanListenToTrackAsync(accountId, bookingPodcastTrack.Id);
                    if (check == false)
                    {
                        throw new HttpRequestException("You are not allowed to listen to this track");
                    }

                    // Check điều kiện nghe và trừ preview
                    if (accountId != booking.PodcastBuddyId)
                    {
                        if (currentBookingStatusId != (int)BookingStatusEnum.TrackPreviewing && currentBookingStatusId != (int)BookingStatusEnum.Completed)
                        {
                            throw new HttpRequestException("Booking with id " + bookingId + " is not in a listenable status");
                        }
                        if (currentBookingProducingRequest.Id.Equals(bookingPodcastTrack.BookingProducingRequestId) == false)
                        {
                            throw new HttpRequestException("Podcast track with id " + podcastTrackId + " does not belong to the current producing request of booking with id " + bookingId);
                        }
                        if (currentBookingStatusId == (int)BookingStatusEnum.TrackPreviewing)
                        {
                            if (bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
                            {
                                throw new HttpRequestException("You have used up all your preview listen slots for podcast track with id " + podcastTrackId);
                            }
                            bookingPodcastTrack.RemainingPreviewListenSlot = bookingPodcastTrack.RemainingPreviewListenSlot - 1;
                            await _bookingPodcastTrackGenericRepository.UpdateAsync(bookingPodcastTrack.Id, bookingPodcastTrack);
                        }
                    }

                    if (sourceType != CustomerListenSessionProcedureSourceDetailTypeEnum.BookingProducingTracks)
                    {
                        throw new HttpRequestException("Invalid SourceType: " + sourceType.ToString());
                    }

                    // Complete existing booking listen session if any
                    var existingBookingListenSession = _bookingPodcastTrackListenSessionGenericRepository.FindAll(
                        predicate: bpts => bpts.AccountId == accountId && bpts.IsCompleted == false
                    ).ToListAsync();
                    foreach (var session in existingBookingListenSession.Result)
                    {
                        session.IsCompleted = true;
                        await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    }

                    // Complete existing episode listen session if any
                    var requestData = new JObject
                    {
                        { "AccountId", accountId },
                        { "IsBookingProducingListenSessionCompleted", false },
                        { "IsEpisodeListenSessionCompleted", true }
                    };
                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: requestData,
                        sagaInstanceId: null,
                        messageName: "all-user-listen-session-completion-flow");
                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, booking.Id.ToString());

                    // Complete exisiting procedure
                    var procedureAffected = await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                    if (procedureAffected > 0)
                    {
                        _logger.LogInformation("Completed {ProcedureAffected} existing listen session procedures for AccountId: {AccountId}", procedureAffected, accountId);
                    }
                    else
                    {
                        _logger.LogInformation("No existing listen session procedures to complete for AccountId: {AccountId}", accountId);
                    }

                    // Create new procedure
                    List<ListenSessionProcedureListenObjectQueueItem> listenObjectSequential = new List<ListenSessionProcedureListenObjectQueueItem>();
                    List<ListenSessionProcedureListenObjectQueueItem> listenObjectRandom = new List<ListenSessionProcedureListenObjectQueueItem>();

                    var count = 0;
                    foreach (var track in currentBookingProducingRequest.BookingPodcastTracks.OrderBy(bp => bp.BookingRequirement.Order))
                    {
                        count += 1;
                        var isListenable = true;
                        if (currentBookingStatusId == (int)BookingStatusEnum.TrackPreviewing)
                        {
                            if (track.RemainingPreviewListenSlot <= 0)
                            {
                                isListenable = false;
                            }
                        }

                        var sequentialItem = new ListenSessionProcedureListenObjectQueueItem
                        {
                            ListenObjectId = track.Id,
                            Order = count,
                            IsListenable = isListenable
                        };
                        listenObjectSequential.Add(sequentialItem);

                        var randomItem = new ListenSessionProcedureListenObjectQueueItem
                        {
                            ListenObjectId = track.Id,
                            Order = count,
                            IsListenable = isListenable
                        };
                        listenObjectRandom.Add(randomItem);
                    }
                    var random = new Random();
                    listenObjectRandom = listenObjectRandom.OrderBy(x => random.Next()).ToList();

                    for (int i = 0; i < listenObjectRandom.Count; i++)
                    {
                        listenObjectRandom[i].Order = i + 1;
                    }

                    var createdProcedure = new CustomerListenSessionProcedure
                    {
                        Id = Guid.NewGuid(),
                        PlayOrderMode = _customerListenSessionProcedureConfig.DefaultPlayOrderMode,
                        IsAutoPlay = _customerListenSessionProcedureConfig.DefaultIsAutoPlay,
                        SourceDetail = new ListenSessionProcedureSourceDetail
                        {
                            Type = sourceType.ToString(),
                            Booking = new BookingInfo
                            {
                                BookingProducingRequestId = currentBookingProducingRequest.Id,
                                Title = booking.Title
                            },
                            PodcastShow = null
                        },
                        ListenObjectsSequentialOrder = listenObjectSequential,
                        ListenObjectsRandomOrder = listenObjectRandom,
                        IsCompleted = false,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    };

                    var isCreatedSuccess = await _customerListenSessionProcedureCachingService.CreateProcedureAsync(accountId, createdProcedure.Id, createdProcedure);
                    if (!isCreatedSuccess)
                    {
                        throw new Exception("An error occurred while creating listen session procedure for booking with id " + bookingId);
                    }

                    // Create new listen session
                    var createdListenSession = new BookingPodcastTrackListenSession
                    {
                        AccountId = accountId,
                        BookingPodcastTrackId = bookingPodcastTrack.Id,
                        LastListenDurationSeconds = 0,
                        IsCompleted = false,
                        ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_bookingListenSessionConfig.SessionExpirationMinutes),
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _bookingPodcastTrackListenSessionGenericRepository.CreateAsync(createdListenSession);

                    await transaction.CommitAsync();

                    var playlistFileKey = FilePathHelper.CombinePaths(
                                        _filePathConfig.BOOKING_FILE_PATH,
                                        booking.Id.ToString(),
                                        currentBookingProducingRequest.Id.ToString(),
                                        bookingPodcastTrack.Id.ToString(),
                                        "playlist",
                                        _hlsConfig.PlaylistFileName
                                    );
                    var bookingListenSession = new BookingTrackListenResponseDTO
                    {
                        Booking = new BookingListenSnippetResponseDTO
                        {
                            Id = booking.Id,
                            Title = booking.Title,
                            Description = booking.Description
                        },
                        BookingPodcastTrack = new BookingPodcastTrackListenSnippetResponseDTO
                        {
                            Id = bookingPodcastTrack.Id,
                            BookingRequirementName = bookingPodcastTrack.BookingRequirement.Name,
                            BookingRequirementDescription = bookingPodcastTrack.BookingRequirement.Description
                        },
                        BookingPodcastTrackListenSession = new BookingPodcastTrackListenSessionSnippetResponseDTO
                        {
                            Id = createdListenSession.Id,
                            LastListenDurationSeconds = createdListenSession.LastListenDurationSeconds
                        },
                        AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
                                ? await _fileIOHelper.GeneratePresignedUrlAsync(
                                    bookingPodcastTrack.AudioFileKey, _bookingListenSessionConfig.SessionAudioUrlExpirationSeconds
                                )
                                : null,
                        PlaylistFileKey = playlistFileKey
                    };

                    return new BookingListenSessionResponseDTO
                    {
                        ListenSession = bookingListenSession,
                        ListenSessionProcedure = createdProcedure
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
                }
            }
        }
        public async Task<byte[]> GetBookingTrackHlsEncryptionKeyFileAsync(int bookingId, Guid podcastTrackId, Guid keyId, int accountId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // validate
                    // var episode = await _podcastEpisodeGenericRepository.FindAll(
                    //     predicate: pe => pe.Id == episodeId && pe.DeletedAt == null && pe.AudioEncryptionKeyId == keyId,
                    //     includeFunc: pe => pe.Include(p => p.PodcastEpisodeStatusTrackings)
                    // ).FirstOrDefaultAsync();

                    // if (episode == null)
                    // {
                    //     throw new Exception("Podcast episode with id " + episodeId + " does not exist, or keyId does not match");
                    // }
                    // else if (episode.DeletedAt != null)
                    // {
                    //     throw new Exception("Podcast episode with id " + episodeId + " has been deleted");
                    // }
                    // else if (episode.PodcastEpisodeStatusTrackings
                    //     .OrderByDescending(pet => pet.CreatedAt)
                    //     .FirstOrDefault()
                    //     .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published)
                    // {
                    //     throw new Exception("Podcast episode with id " + episodeId + " is not in Published status");
                    // }

                    var booking = await _bookingGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings)
                        .Include(b => b.BookingProducingRequests))
                        .Where(b => b.Id == bookingId)
                        .FirstOrDefaultAsync();

                    if (booking == null)
                    {
                        throw new Exception("Booking with id " + bookingId + " does not exist");
                    }
                    var currentBookingStatusId = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault()
                        .BookingStatusId;

                    var currentBookingProducingRequest = booking.BookingProducingRequests
                        .Where(bpr => bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();

                    var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.Id == podcastTrackId && bpt.AudioEncryptionKeyId == keyId
                    ).FirstOrDefaultAsync();

                    if (bookingPodcastTrack == null)
                    {
                        throw new Exception("Podcast track with id " + podcastTrackId + " does not exist, or keyId does not match");
                    }

                    var check = await CheckListenerCanListenToTrackIgnorePreviewingAsync(accountId, bookingPodcastTrack.Id);
                    if (check == false)
                    {
                        throw new HttpRequestException("You are not allowed to listen to this track");
                    }

                    if (accountId != booking.PodcastBuddyId)
                    {
                        if (currentBookingStatusId != (int)BookingStatusEnum.TrackPreviewing && currentBookingStatusId != (int)BookingStatusEnum.Completed)
                        {
                            throw new HttpRequestException("Booking with id " + bookingId + " is not in a listenable status");
                        }
                        if (currentBookingProducingRequest.Id.Equals(bookingPodcastTrack.BookingProducingRequestId) == false)
                        {
                            throw new HttpRequestException("Podcast track with id " + podcastTrackId + " does not belong to the current producing request of booking with id " + bookingId);
                        }
                        //if (currentBookingStatusId == (int)BookingStatusEnum.TrackPreviewing)
                        //{
                        //    if (bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
                        //    {
                        //        throw new HttpRequestException("You have used up all your preview listen slots for podcast track with id " + podcastTrackId);
                        //    }
                        //    bookingPodcastTrack.RemainingPreviewListenSlot = bookingPodcastTrack.RemainingPreviewListenSlot - 1;
                        //    await _bookingPodcastTrackGenericRepository.UpdateAsync(bookingPodcastTrack.Id, bookingPodcastTrack);
                        //}
                    }

                    await transaction.CommitAsync();

                    // trả về EncryptionKey file bytes
                    return await _fileIOHelper.GetFileBytesAsync(bookingPodcastTrack.AudioEncryptionKeyFileKey);

                    //return Array.Empty<byte>(); // xoá cái này

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
                }
            }
        }
        public async Task<BookingListenSessionResponseDTO> GetLatestBookingPodcastTrackListenSessionAsync(int accountId, DeviceInfoDTO deviceInfo)
        {
            try
            {
                var bookingPodcastTrackListenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll(
                    predicate: bpts => bpts.AccountId == accountId && !bpts.IsCompleted,
                    includeFunc: bpts => bpts
                        .Include(bpts => bpts.BookingPodcastTrack)
                        .ThenInclude(bpt => bpt.BookingRequirement)
                )
                .OrderByDescending(bpts => bpts.CreatedAt)
                .FirstOrDefaultAsync();

                Console.WriteLine("\n ALOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
                Console.WriteLine(bookingPodcastTrackListenSession == null);
                //Console.WriteLine("\n" + JsonConvert.SerializeObject(bookingPodcastTrackListenSession) + "\n");
                if (bookingPodcastTrackListenSession == null)
                {
                    return new BookingListenSessionResponseDTO
                    {
                        ListenSession = null,
                        ListenSessionProcedure = null
                    };
                }
                else
                {
                    var bookingPodcastTrack = bookingPodcastTrackListenSession.BookingPodcastTrack;
                    var booking = await _bookingGenericRepository.FindAll(
                        predicate: b => b.Id == bookingPodcastTrack.BookingId,
                        includeFunc: b => b
                            .Include(b => b.BookingStatusTrackings)
                            .Include(b => b.BookingProducingRequests)
                    ).FirstOrDefaultAsync();

                    var currentStatus = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault()
                        .BookingStatusId;

                    // Check điều kiện nghe
                    if (currentStatus != (int)BookingStatusEnum.TrackPreviewing && currentStatus != (int)BookingStatusEnum.Completed)
                    {
                        return new BookingListenSessionResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = null
                        };
                    }

                    var playlistFileKey = FilePathHelper.CombinePaths(
                                        _filePathConfig.BOOKING_FILE_PATH,
                                        booking.Id.ToString(),
                                        bookingPodcastTrack.BookingProducingRequestId.ToString(),
                                        bookingPodcastTrack.Id.ToString(),
                                        "playlist",
                                        _hlsConfig.PlaylistFileName
                                    );
                    var bookingListenSession = new BookingTrackListenResponseDTO
                    {
                        Booking = new BookingListenSnippetResponseDTO
                        {
                            Id = booking.Id,
                            Title = booking.Title,
                            Description = booking.Description
                        },
                        BookingPodcastTrack = new BookingPodcastTrackListenSnippetResponseDTO
                        {
                            Id = bookingPodcastTrack.Id,
                            BookingRequirementName = bookingPodcastTrack.BookingRequirement.Name,
                            BookingRequirementDescription = bookingPodcastTrack.BookingRequirement.Description
                        },
                        BookingPodcastTrackListenSession = new BookingPodcastTrackListenSessionSnippetResponseDTO
                        {
                            Id = bookingPodcastTrackListenSession.Id,
                            LastListenDurationSeconds = bookingPodcastTrackListenSession.LastListenDurationSeconds
                        },
                        AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
                                ? await _fileIOHelper.GeneratePresignedUrlAsync(
                                    bookingPodcastTrack.AudioFileKey, _bookingListenSessionConfig.SessionAudioUrlExpirationSeconds
                                )
                                : null,
                        PlaylistFileKey = playlistFileKey
                    };
                    // Refresh procedure queues
                    var listenSessionProcedure = await _customerListenSessionProcedureCachingService.GetActiveProcedureByCustomerIdAsync(accountId);
                    listenSessionProcedure.ListenObjectsSequentialOrder = await RefreshSequentialOrderQueue(listenSessionProcedure.ListenObjectsSequentialOrder);
                    listenSessionProcedure.ListenObjectsRandomOrder = await RefreshRandomOrderQueue(listenSessionProcedure.ListenObjectsRandomOrder);

                    await _customerListenSessionProcedureCachingService.UpdateProcedureAsync(accountId, listenSessionProcedure.Id, listenSessionProcedure);
                    return new BookingListenSessionResponseDTO
                    {
                        ListenSession = bookingListenSession,
                        ListenSessionProcedure = listenSessionProcedure
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting latest booking podcast track listen session for AccountId: {AccountId}", accountId);
                throw new HttpRequestException("Error occurred while retrieving listen session, error: " + ex.Message);
            }
        }
        public async Task UpdateBookingListenSessionDurationAsync(UpdateBookingListenSessionDurationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var transactionComplete = false;
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;
                    
                    var bookingPodcastTrackListenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindByIdAsync(
                        id: parameter.BookingPodcastTrackListenSessionId,
                        includeFunc: bpts => bpts.Include(bpts => bpts.BookingPodcastTrack)
                        .ThenInclude(bpt => bpt.BookingProducingRequest)
                    );
                    if (bookingPodcastTrackListenSession == null)
                    {
                        throw new Exception("BookingPodcastTrackListenSession not found for Id: " + parameter.BookingPodcastTrackListenSessionId);
                    }

                    var check = await CheckListenerCanListenToTrackIgnorePreviewingAsync(parameter.ListenerId, bookingPodcastTrackListenSession.BookingPodcastTrack.Id);
                    if (check == false)
                    {
                        bookingPodcastTrackListenSession.IsCompleted = true;
                        await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(bookingPodcastTrackListenSession.Id, bookingPodcastTrackListenSession);
                        transactionComplete = true;
                        await transaction.CommitAsync();
                        throw new Exception("This audio track is no longer eligible for listening");
                    }

                    var lastestListenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll(
                        predicate: bpts => bpts.AccountId == parameter.ListenerId,
                        includeFunc: bpts => bpts
                            .Include(bpts => bpts.BookingPodcastTrack)
                            .ThenInclude(bpt => bpt.BookingProducingRequest)
                    ).OrderByDescending(bpts => bpts.CreatedAt).FirstOrDefaultAsync();

                    if (lastestListenSession != null && lastestListenSession.Id == bookingPodcastTrackListenSession.Id && lastestListenSession.ExpiredAt <= _dateHelper.GetNowByAppTimeZone())
                    {
                        bookingPodcastTrackListenSession.IsCompleted = false;
                        bookingPodcastTrackListenSession.LastListenDurationSeconds = parameter.LastListenDurationSeconds;
                        bookingPodcastTrackListenSession.ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_bookingListenSessionConfig.SessionAdditionalUpdateBufferExpirationMinutes);
                    }
                    else
                    {
                        bookingPodcastTrackListenSession.LastListenDurationSeconds = parameter.LastListenDurationSeconds;
                    }
                    await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(bookingPodcastTrackListenSession.Id, bookingPodcastTrackListenSession);

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Successfully updated booking listen session duration for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    if (!transactionComplete)
                    {
                        await transaction.RollbackAsync();
                    }
                    _logger.LogError(ex, "Error occurred while updating booking listen session duration for BookingPodcastTrackListenSessionId: {BookingPodcastTrackListenSessionId}", parameter.BookingPodcastTrackListenSessionId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Updating booking listen session duration failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Updating booking listen session duration failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task<BookingListenSessionResponseDTO> NavigateBookingPodcastTrackListenSessionAsync(int accountId, DeviceInfoDTO deviceInfo, Guid listenSessionId, Guid procedureId, ListenSessionNavigateTypeEnum navigateType)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    BookingListenSessionResponseDTO result = null;

                    var procedure = await _customerListenSessionProcedureCachingService.GetProcedureAsync(accountId, procedureId);
                    procedure.ListenObjectsSequentialOrder = await RefreshSequentialOrderQueue(procedure.ListenObjectsSequentialOrder);
                    procedure.ListenObjectsRandomOrder = await RefreshRandomOrderQueue(procedure.ListenObjectsRandomOrder);
                    await _customerListenSessionProcedureCachingService.UpdateProcedureAsync(accountId, procedureId, procedure);

                    var listenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindByIdAsync(listenSessionId,
                        includeFunc: function => function
                        .Include(bpt => bpt.BookingPodcastTrack));

                    //&& procedure.ListenObjectsSequentialOrder.Any(o => o.IsListenable) ||
                    //    procedure.PlayOrderMode.Equals(CustomerListenSessionProcedurePlayOrderModeEnum.Random.ToString()) && procedure.ListenObjectsRandomOrder.Any(o => o.IsListenable))
                    //{
                    //    procedure.
                    //}

                    ListenSessionProcedureListenObjectQueueItem nextListenObj = null;
                    if (procedure.PlayOrderMode.Equals(CustomerListenSessionProcedurePlayOrderModeEnum.Sequential.ToString()))
                    {
                        var currentListenObj = procedure.ListenObjectsSequentialOrder.Where(o => o.ListenObjectId.Equals(listenSession.BookingPodcastTrackId)).FirstOrDefault();
                        if (currentListenObj == null)
                        {
                            throw new HttpRequestException("Current listen session's track is not found in the procedure's listen object queue");
                        }
                        if (procedure.ListenObjectsSequentialOrder.Count(o => o.IsListenable) > 1)
                        {
                            await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                            await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(accountId, procedureId, false);
                            if (navigateType == ListenSessionNavigateTypeEnum.Next)
                            {
                                nextListenObj = procedure.ListenObjectsSequentialOrder.OrderBy(o => o.Order).Where(o => o.IsListenable && o.Order > currentListenObj.Order).FirstOrDefault();
                                if (nextListenObj == null)
                                {
                                    nextListenObj = procedure.ListenObjectsSequentialOrder.OrderBy(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                }
                            }
                            else if (navigateType == ListenSessionNavigateTypeEnum.Previous)
                            {
                                nextListenObj = procedure.ListenObjectsSequentialOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable && o.Order < currentListenObj.Order).FirstOrDefault();
                                if (nextListenObj == null)
                                {
                                    nextListenObj = procedure.ListenObjectsSequentialOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                }
                            }
                        }
                        else if (procedure.ListenObjectsSequentialOrder.Count(o => o.IsListenable) == 1)
                        {
                            if (procedure.ListenObjectsSequentialOrder.Where(o => o.ListenObjectId.Equals(listenSession.BookingPodcastTrackId) && o.IsListenable).IsNullOrEmpty())
                            {
                                await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                                await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(accountId, procedureId, false);
                                if (navigateType == ListenSessionNavigateTypeEnum.Next)
                                {
                                    nextListenObj = procedure.ListenObjectsSequentialOrder.OrderBy(o => o.Order).Where(o => o.IsListenable && o.Order > currentListenObj.Order).FirstOrDefault();
                                    if (nextListenObj == null)
                                    {
                                        nextListenObj = procedure.ListenObjectsSequentialOrder.OrderBy(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                    }
                                }
                                else if (navigateType == ListenSessionNavigateTypeEnum.Previous)
                                {
                                    nextListenObj = procedure.ListenObjectsSequentialOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable && o.Order < currentListenObj.Order).FirstOrDefault();
                                    if (nextListenObj == null)
                                    {
                                        nextListenObj = procedure.ListenObjectsSequentialOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                    }
                                }
                            }
                        }
                        else
                        {
                            await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                            await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(accountId, procedureId, false);
                        }
                    }
                    else if (procedure.PlayOrderMode.Equals(CustomerListenSessionProcedurePlayOrderModeEnum.Random.ToString()))
                    {
                        var currentListenObj = procedure.ListenObjectsRandomOrder.Where(o => o.ListenObjectId.Equals(listenSession.BookingPodcastTrackId)).FirstOrDefault();
                        if (currentListenObj == null)
                        {
                            throw new HttpRequestException("Current listen session's track is not found in the procedure's listen object queue");
                        }
                        if (procedure.ListenObjectsRandomOrder.Count(o => o.IsListenable) > 1)
                        {
                            await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                            await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(accountId, procedureId, false);
                            if (navigateType == ListenSessionNavigateTypeEnum.Next)
                            {
                                nextListenObj = procedure.ListenObjectsRandomOrder.OrderBy(o => o.Order).Where(o => o.IsListenable && o.Order > currentListenObj.Order).FirstOrDefault();
                                if (nextListenObj == null)
                                {
                                    nextListenObj = procedure.ListenObjectsRandomOrder.OrderBy(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                }
                            }
                            else if (navigateType == ListenSessionNavigateTypeEnum.Previous)
                            {
                                nextListenObj = procedure.ListenObjectsRandomOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable && o.Order < currentListenObj.Order).FirstOrDefault();
                                if (nextListenObj == null)
                                {
                                    nextListenObj = procedure.ListenObjectsRandomOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                }
                            }
                        }
                        else if (procedure.ListenObjectsRandomOrder.Count(o => o.IsListenable) == 1)
                        {
                            if (procedure.ListenObjectsRandomOrder.Where(o => o.ListenObjectId.Equals(listenSession.BookingPodcastTrackId) && o.IsListenable).IsNullOrEmpty())
                            {
                                await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                                await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(accountId, procedureId, false);
                                if (navigateType == ListenSessionNavigateTypeEnum.Next)
                                {
                                    nextListenObj = procedure.ListenObjectsRandomOrder.OrderBy(o => o.Order).Where(o => o.IsListenable && o.Order > currentListenObj.Order).FirstOrDefault();
                                    if (nextListenObj == null)
                                    {
                                        nextListenObj = procedure.ListenObjectsRandomOrder.OrderBy(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                    }
                                }
                                else if (navigateType == ListenSessionNavigateTypeEnum.Previous)
                                {
                                    nextListenObj = procedure.ListenObjectsRandomOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable && o.Order < currentListenObj.Order).FirstOrDefault();
                                    if (nextListenObj == null)
                                    {
                                        nextListenObj = procedure.ListenObjectsRandomOrder.OrderByDescending(o => o.Order).Where(o => o.IsListenable).FirstOrDefault();
                                    }
                                }
                            }
                        }
                        else
                        {
                            await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(accountId);
                            await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(accountId, procedureId, false);
                        }
                    }


                    if (nextListenObj == null)
                    {
                        result = new BookingListenSessionResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = procedure
                        };
                    }
                    else
                    {
                        // Complete all existing listen session
                        var existingListenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll()
                            .Where(bptls => bptls.AccountId == accountId && !bptls.IsCompleted)
                            .ToListAsync();
                        if (existingListenSession != null)
                        {
                            foreach (var bptls in existingListenSession)
                            {
                                bptls.IsCompleted = true;
                                await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(bptls.Id, bptls);
                            }
                        }

                        var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindByIdAsync(nextListenObj.ListenObjectId,
                            includeFunc: function => function
                            .Include(bp => bp.BookingRequirement));

                        var booking = await _bookingGenericRepository.FindAll(
                            predicate: b => b.Id == bookingPodcastTrack.BookingId,
                            includeFunc: b => b
                                .Include(b => b.BookingStatusTrackings)
                        ).FirstOrDefaultAsync();

                        Console.WriteLine("---- BookingPodcastTrack Id to navigate to: " + bookingPodcastTrack.Id);

                        if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != (int)BookingStatusEnum.Completed)
                        {
                            bookingPodcastTrack.RemainingPreviewListenSlot -= 1;
                            await _bookingPodcastTrackGenericRepository.UpdateAsync(bookingPodcastTrack.Id, bookingPodcastTrack);
                        }

                        var newListenSession = new BookingPodcastTrackListenSession
                        {
                            AccountId = accountId,
                            BookingPodcastTrackId = nextListenObj.ListenObjectId,
                            LastListenDurationSeconds = 0,
                            IsCompleted = false,
                            ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_bookingListenSessionConfig.SessionExpirationMinutes),
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var createdListenSession = await _bookingPodcastTrackListenSessionGenericRepository.CreateAsync(newListenSession);

                        Console.WriteLine("---- Created Listen Session Id: " + createdListenSession.Id);

                        var playlistFileKey = FilePathHelper.CombinePaths(
                                            _filePathConfig.BOOKING_FILE_PATH,
                                            booking.Id.ToString(),
                                            bookingPodcastTrack.BookingProducingRequestId.ToString(),
                                            bookingPodcastTrack.Id.ToString(),
                                            "playlist",
                                            _hlsConfig.PlaylistFileName
                                        );
                        var bookingListenSession = new BookingTrackListenResponseDTO
                        {
                            Booking = new BookingListenSnippetResponseDTO
                            {
                                Id = booking.Id,
                                Title = booking.Title,
                                Description = booking.Description
                            },
                            BookingPodcastTrack = new BookingPodcastTrackListenSnippetResponseDTO
                            {
                                Id = bookingPodcastTrack.Id,
                                BookingRequirementName = bookingPodcastTrack.BookingRequirement.Name,
                                BookingRequirementDescription = bookingPodcastTrack.BookingRequirement.Description
                            },
                            BookingPodcastTrackListenSession = new BookingPodcastTrackListenSessionSnippetResponseDTO
                            {
                                Id = createdListenSession.Id,
                                LastListenDurationSeconds = createdListenSession.LastListenDurationSeconds
                            },
                            AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
                                    ? await _fileIOHelper.GeneratePresignedUrlAsync(
                                        bookingPodcastTrack.AudioFileKey, _bookingListenSessionConfig.SessionAudioUrlExpirationSeconds
                                    )
                                    : null,
                            PlaylistFileKey = playlistFileKey
                        };
                        result = new BookingListenSessionResponseDTO
                        {
                            ListenSession = bookingListenSession,
                            ListenSessionProcedure = procedure
                        };

                        var requestData = new JObject
                        {
                            { "AccountId", accountId },
                            { "IsBookingProducingListenSessionCompleted", false },
                            { "IsEpisodeListenSessionCompleted", true }
                        };
                        var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.ContentManagementDomain,
                            requestData: requestData,
                            sagaInstanceId: null,
                            messageName: "all-user-listen-session-completion-flow");
                        await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, booking.Id.ToString());
                    }
                    await transaction.CommitAsync();

                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while navigating, error: " + ex.Message);
                }
            }
        }
        public async Task ExpireBookingPodcastTrackListenSessionsAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy tất cả listensession có iscompleted =false và expiredat <= now
                    var now = _dateHelper.GetNowByAppTimeZone();
                    var sessionsToExpire = await _bookingPodcastTrackListenSessionGenericRepository.FindAll(
                        predicate: btls => btls.IsCompleted == false && btls.ExpiredAt <= now
                    ).ToListAsync();

                    foreach (var session in sessionsToExpire)
                    {
                        session.IsCompleted = true;
                        await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new Exception("Expire booking podcast track listen sessions job failed, error: " + ex.Message);
                }
            }
        }
        public async Task CompleteAllUserBookingProducingListenSessionsAsync(CompleteAllUserBookingProducingListenSessionsParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    if (parameter.IsBookingProducingListenSessionCompleted)
                    {
                        var bookingListenSessions = await _bookingPodcastTrackListenSessionGenericRepository.FindAll(
                            predicate: bpts => bpts.AccountId == parameter.AccountId && !bpts.IsCompleted
                        ).ToListAsync();
                        foreach (var session in bookingListenSessions)
                        {
                            session.IsCompleted = true;
                            await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                        }

                        var procedure = await _customerListenSessionProcedureCachingService.GetActiveProcedureByCustomerIdAsync(parameter.AccountId);
                        if (procedure != null)
                        {
                            if (procedure.SourceDetail.Type == CustomerListenSessionProcedureSourceDetailTypeEnum.BookingProducingTracks.ToString() && !procedure.IsCompleted)
                            {
                                await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(parameter.AccountId, procedure.Id, true);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Successfully Complete All User Booking Producing Listen Sessions for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Complete All User Booking Producing Listen Sessions for AccountId: {AccountId}", parameter.AccountId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Complete All User Booking Producing Listen Sessions failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Complete All User Booking Producing Listen Sessions failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task RemovePodcasterBookingTracksListenSessionTerminatePodcasterForceAsync(RemovePodcasterBookingTracksListenSessionTerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var bookings = await _bookingGenericRepository.FindAll()
                        .Include(b => b.BookingProducingRequests)
                            .ThenInclude(bp => bp.BookingPodcastTracks) // Fix: Include the missing navigation property
                        .Where(b => b.PodcastBuddyId == parameter.PodcasterId || b.AccountId == parameter.PodcasterId)
                        .ToListAsync();

                    if (bookings.Any()) // Fix: Use Any() instead of null check
                    {
                        foreach (var booking in bookings)
                        {
                            var finishedProducingRequests = booking.BookingProducingRequests
                                .Where(bp => bp.FinishedAt != null)
                                .ToList();
                            foreach (var producingRequest in finishedProducingRequests)
                            {
                                var trackIds = producingRequest.BookingPodcastTracks.Select(bp => bp.Id).ToList();
                                if (trackIds.Any())
                                {
                                    // Complete listen sessions
                                    var listenSessions = await _bookingPodcastTrackListenSessionGenericRepository.FindAll()
                                        .Where(ls => !ls.IsCompleted
                                                    && trackIds.Contains(ls.BookingPodcastTrackId)
                                                    && (ls.AccountId == booking.AccountId || ls.AccountId == parameter.PodcasterId))
                                        .ToListAsync();

                                    foreach (var session in listenSessions)
                                    {
                                        session.IsCompleted = true;
                                        await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                                    }
                                    // Complete customer listen session procedure in cache
                                    var customerListenSessionProcedure = await _customerListenSessionProcedureCachingService
                                        .GetActiveProcedureByCustomerIdAsync(booking.AccountId);

                                    if (customerListenSessionProcedure != null
                                        && !customerListenSessionProcedure.IsCompleted && customerListenSessionProcedure.SourceDetail.Booking != null)
                                    {
                                        if(customerListenSessionProcedure.SourceDetail.Booking.BookingProducingRequestId == producingRequest.Id)
                                        await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(booking.AccountId, customerListenSessionProcedure.Id, true);
                                    }
                                }
                            }
                        }
                    }
                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Successfully Remove Podcaster Booking Tracks Listen Session Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Remove Podcaster Booking Tracks Listen Session Terminate Podcaster Force for Podcaster: {PodcasterId}", parameter.PodcasterId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Remove Podcaster Booking Tracks Listen Session Terminate Podcaster Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Complete All User Booking Producing Listen Sessions failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task<List<BookingHoldingListItemResponseDTO>> GetAllHoldingBookingsAsync()
        {
            try
            {
                var bookings = await _bookingGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(b => b.BookingStatusTrackings)
                    .ThenInclude(b => b.BookingStatus))
                    .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != null &&
                    !new[] {
                        (int)BookingStatusEnum.QuotationRequest,
                        (int)BookingStatusEnum.QuotationDealing,
                        (int)BookingStatusEnum.QuotationCancelled,
                        (int)BookingStatusEnum.QuotationRejected,
                        (int)BookingStatusEnum.Completed,
                        (int)BookingStatusEnum.CancelledManually,
                        (int)BookingStatusEnum.CancelledAutomatically
                    }.Contains(b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId))
                    .ToListAsync();
                if(bookings.IsNullOrEmpty())
                {
                    return new List<BookingHoldingListItemResponseDTO>();
                }
                var result = new List<BookingHoldingListItemResponseDTO>();

                foreach (var booking in bookings)
                {
                    var holdingAmount = await GetBookingHoldingTransactionByBookingId(booking.Id);
                    var account = await _accountCachingService.GetAccountStatusCacheById(booking.AccountId);
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(booking.PodcastBuddyId);
                    AccountStatusCache? assignedStaff = null;
                    if (booking.AssignedStaffId != null)
                    {
                        assignedStaff = await _accountCachingService.GetAccountStatusCacheById(booking.AssignedStaffId.Value);
                    }
                    result.Add(new BookingHoldingListItemResponseDTO
                    {
                        Id = booking.Id,
                        Title = booking.Title,
                        Description = booking.Description,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastBuddy = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        AssignedStaff = assignedStaff != null ? new AccountSnippetResponseDTO
                        {
                            Id = assignedStaff.Id,
                            FullName = assignedStaff.FullName,
                            Email = assignedStaff.Email,
                            MainImageFileKey = assignedStaff.MainImageFileKey
                        } : null,
                        DeadlineDays = booking.DeadlineDays,
                        Price = booking.Price,
                        Deadline = booking.Deadline,
                        DemoAudioFileKey = booking.DemoAudioFileKey,
                        BookingManualCancelledReason = booking.BookingManualCancelledReason,
                        BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                        CreatedAt = booking.CreatedAt,
                        UpdatedAt = booking.UpdatedAt,
                        CurrentStatus = new BookingStatusResponseDTO
                        {
                            Id = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatusId,
                            Name = booking.BookingStatusTrackings
                                .OrderByDescending(bst => bst.CreatedAt)
                                .FirstOrDefault().BookingStatus.Name
                        },
                        HoldingAmount = holdingAmount != null ? holdingAmount.Amount : 0m
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all holding bookings");
                throw new HttpRequestException("Retreive holding bookings failed. Error: " + ex.Message);
            }
        }
        private async Task<List<AccountDTO>?> GetStaffList()
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeStaffList",
                            QueryType = "findall",
                            EntityType = "Account",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsVerify = true,
                                        RoleId = (int)RoleEnum.Staff,
                                        DeactivatedAt = (DateTime?) null
                                    },
                                }),
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

                return result.Results?["activeStaffList"] is JArray staffListArray && staffListArray.Count >= 0
                    ? staffListArray.ToObject<List<AccountDTO>>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching staff list from User Service");
                throw new HttpRequestException("Retreive Staff list failed. Error: " + ex.Message);
            }
        }
        private async Task<bool> CheckListenerCanListenToTrackAsync(int accountId, Guid podcastTrackId)
        {
            try
            {
                var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.Id == podcastTrackId,
                        includeFunc: bpt => bpt.Include(b => b.BookingRequirement)
                        .Include(b => b.BookingProducingRequest)
                        .ThenInclude(bpr => bpr.Booking)
                        .ThenInclude(bpr => bpr.BookingStatusTrackings)
                    ).FirstOrDefaultAsync();

                var producingRequest = bookingPodcastTrack.BookingProducingRequest;
                var booking = bookingPodcastTrack.BookingProducingRequest.Booking;
                var bookingStatus = booking.BookingStatusTrackings
                    .OrderByDescending(bst => bst.CreatedAt)
                    .FirstOrDefault().BookingStatusId;

                if (bookingStatus != (int)BookingStatusEnum.TrackPreviewing && bookingStatus != (int)BookingStatusEnum.Completed)
                {
                    return false;
                }
                if (bookingStatus == (int)BookingStatusEnum.TrackPreviewing)
                {
                    if (bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
                    {
                        return false;
                    }
                }
                var currentBookingProducingRequest = booking.BookingProducingRequests
                        .Where(bpr => bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();
                if (currentBookingProducingRequest != null)
                    if (producingRequest.Id != currentBookingProducingRequest.Id)
                        return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if listener can listen to track, error: " + ex.Message);
                return false;
            }
        }
        public async Task ProcessBookingDepositPaymentAsync(ProcessBookingDepositPaymentParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        id: parameter.BookingId,
                        includeFunc: b => b
                            .Include(b => b.BookingStatusTrackings)
                    );
                    if (booking == null)
                    {
                        throw new Exception("Booking not found for Id: " + parameter.BookingId);
                    }
                    var currentBookingStatus = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault().BookingStatusId;
                    if (currentBookingStatus != (int)BookingStatusEnum.QuotationDealing)
                    {
                        throw new Exception("Booking is not in QuotationDealing status, cannot process deposit payment");
                    }

                    if (booking.AccountId != parameter.AccountId)
                    {
                        throw new Exception("Booking does not belong to the account, cannot process deposit payment");
                    }

                    var config = await GetActiveSystemConfigProfile();
                    if (config == null)
                    {
                        throw new Exception("System configuration not found");
                    }
                    var depositRate = config.BookingConfig.DepositRate;
                    var depositAmount = booking.Price * (decimal)depositRate;

                    var account = await GetAccount(booking.AccountId);
                    if(account == null)
                    {
                        throw new Exception("Account not found for Id: " + booking.AccountId);
                    }
                    if(account.Balance - depositAmount < 0)
                    {
                        throw new Exception("Insufficient balance to process deposit payment");
                    }

                    //var paymentRequestData = new JObject
                    //{
                    //    { "BookingId", parameter.BookingId },
                    //    { "AccountId", parameter.AccountId },
                    //    { "Amount", depositAmount },
                    //    { "TransactionTypeId", (int)TransactionTypeEnum.BookingDeposit }
                    //};
                    //var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                    //    topic: KafkaTopicEnum.PaymentProcessingDomain,
                    //    requestData: paymentRequestData,
                    //    sagaInstanceId: null,
                    //    messageName: "booking-transaction-deposit-payment-flow");
                    //var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

                    await transaction.CommitAsync();

                    var newRequestData = command.RequestData;
                    newRequestData["Amount"] = depositAmount;
                    newRequestData["TransactionTypeId"] = (int)TransactionTypeEnum.BookingDeposit;
                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Successfully Processing booking deposit payment for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Processing booking deposit payment for AccountId: {AccountId}", parameter.AccountId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Processing booking deposit payment failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Processing booking deposit payment failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task ProcessBookingPayTheRestPaymentAsync(ProcessBookingPayTheRestPaymentParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        id: parameter.BookingId,
                        includeFunc: b => b
                            .Include(b => b.BookingStatusTrackings)
                    );
                    if (booking == null)
                    {
                        throw new Exception("Booking not found for Id: " + parameter.BookingId);
                    }
                    var currentBookingStatus = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault().BookingStatusId;
                    if (currentBookingStatus != (int)BookingStatusEnum.TrackPreviewing)
                    {
                        throw new Exception("Booking is not in TrackPreviewing status, cannot process deposit payment");
                    }

                    if (booking.AccountId != parameter.AccountId)
                    {
                        throw new Exception("Booking does not belong to the account, cannot process deposit payment");
                    }

                    var config = await GetActiveSystemConfigProfile();
                    if (config == null)
                    {
                        throw new Exception("System configuration not found");
                    }
                    var depositRate = config.BookingConfig.DepositRate;
                    var depositAmount = booking.Price * (decimal)depositRate;
                    var payTheRestAmount = booking.Price - depositAmount;

                    var account = await GetAccount(booking.AccountId);
                    if (account == null)
                    {
                        throw new Exception("Account not found for Id: " + booking.AccountId);
                    }
                    if (account.Balance - payTheRestAmount < 0)
                    {
                        throw new Exception("Insufficient balance to process deposit payment");
                    }

                    //var paymentRequestData = new JObject
                    //{
                    //    { "BookingId", parameter.BookingId },
                    //    { "AccountId", parameter.AccountId },
                    //    { "PodcasterId", booking.PodcastBuddyId },
                    //    { "Amount", payTheRestAmount },
                    //    { "TransactionTypeId", (int)TransactionTypeEnum.BookingPayTheRest }
                    //};
                    //var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                    //    topic: KafkaTopicEnum.PaymentProcessingDomain,
                    //    requestData: paymentRequestData,
                    //    sagaInstanceId: null,
                    //    messageName: "booking-final-payment-flow");
                    //var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

                    await transaction.CommitAsync();

                    var newRequestData = command.RequestData;
                    newRequestData["PodcasterId"] = booking.PodcastBuddyId;
                    newRequestData["Amount"] = depositAmount;
                    newRequestData["TransactionTypeId"] = (int)TransactionTypeEnum.BookingPayTheRest;
                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Successfully Processing booking pay the rest payment for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Processing booking pay the rest payment for AccountId: {AccountId}", parameter.AccountId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Processing booking pay the rest payment failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Processing booking pay the rest payment failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        private async Task<bool> CheckListenerCanListenToTrackIgnorePreviewingAsync(int accountId, Guid podcastTrackId)
        {
            try
            {
                var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.Id == podcastTrackId,
                        includeFunc: bpt => bpt.Include(b => b.BookingRequirement)
                        .Include(b => b.BookingProducingRequest)
                        .ThenInclude(bpr => bpr.Booking)
                        .ThenInclude(bpr => bpr.BookingStatusTrackings)
                    ).FirstOrDefaultAsync();

                var producingRequest = bookingPodcastTrack.BookingProducingRequest;
                var booking = bookingPodcastTrack.BookingProducingRequest.Booking;
                var bookingStatus = booking.BookingStatusTrackings
                    .OrderByDescending(bst => bst.CreatedAt)
                    .FirstOrDefault().BookingStatusId;

                if (bookingStatus != (int)BookingStatusEnum.TrackPreviewing && bookingStatus != (int)BookingStatusEnum.Completed)
                {
                    return false;
                }
                var currentBookingProducingRequest = booking.BookingProducingRequests
                        .Where(bpr => bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();
                if (currentBookingProducingRequest != null)
                    if (producingRequest.Id != currentBookingProducingRequest.Id)
                        return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if listener can listen to track");
                return false;
            }
        }
        private async Task<List<ListenSessionProcedureListenObjectQueueItem>> RefreshSequentialOrderQueue(List<ListenSessionProcedureListenObjectQueueItem> list)
        {
            try
            {
                foreach (var item in list)
                {
                    var track = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.Id == item.ListenObjectId,
                        includeFunc: bpt => bpt.Include(bpt => bpt.BookingProducingRequest)
                    ).FirstOrDefaultAsync();
                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        id: track.BookingId,
                        includeFunc: b => b
                            .Include(b => b.BookingProducingRequests)
                            .Include(b => b.BookingStatusTrackings));
                    var lastestProducingRequest = booking.BookingProducingRequests
                        .Where(bpr => bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();
                    if (lastestProducingRequest.Id != track.BookingProducingRequestId)
                    {
                        item.IsListenable = false;
                        continue;
                    }
                    if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != (int)BookingStatusEnum.Completed)
                    {
                        if (track == null || track.RemainingPreviewListenSlot <= 0)
                        {
                            item.IsListenable = false;
                        }
                        else
                        {
                            if(booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != (int)BookingStatusEnum.TrackPreviewing)
                                item.IsListenable = false;
                            else
                                item.IsListenable = true;
                        }
                    }
                    else
                    {
                        item.IsListenable = true;
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing sequential order queue");
                throw new HttpRequestException("Error occurred while refreshing sequential order queue, error: " + ex.Message);
            }
        }
        private async Task<List<ListenSessionProcedureListenObjectQueueItem>> RefreshRandomOrderQueue(List<ListenSessionProcedureListenObjectQueueItem> list)
        {
            try
            {
                foreach (var item in list)
                {
                    var track = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.Id == item.ListenObjectId,
                        includeFunc: bpt => bpt.Include(bpt => bpt.BookingProducingRequest)
                    ).FirstOrDefaultAsync();
                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        id: track.BookingId,
                        includeFunc: b => b
                            .Include(b => b.BookingProducingRequests)
                            .Include(b => b.BookingStatusTrackings));
                    var lastestProducingRequest = booking.BookingProducingRequests
                        .Where(bpr => bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();
                    if (lastestProducingRequest.Id != track.BookingProducingRequestId)
                    {
                        item.IsListenable = false;
                        continue;
                    }
                    if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != (int)BookingStatusEnum.Completed)
                    {
                        if (track == null || track.RemainingPreviewListenSlot <= 0)
                        {
                            item.IsListenable = false;
                        }
                        else
                        {
                            if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).FirstOrDefault().BookingStatusId != (int)BookingStatusEnum.TrackPreviewing)
                                item.IsListenable = false;
                            else
                                item.IsListenable = true;
                        }
                    }
                    else
                    {
                        item.IsListenable = true;
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing random order queue");
                throw new HttpRequestException("Error occurred while refreshing random order queue, error: " + ex.Message);
            }
        }
        public async Task<AccountDTO?> GetAccount(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",
                        Parameters = JObject.FromObject(new
                        {
                            id = accountId
                        }),
                        Fields = new[] {
                            "Id",
                            "Email",
                            "Password",
                            "RoleId",
                            "FullName",
                            "Dob",
                            "Gender",
                            "Address",
                            "Phone",
                            "Balance"
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

            if (result.Results["account"] == null) return null;

            return (result.Results["account"] as JObject).ToObject<AccountDTO>();
        }
    }
}
