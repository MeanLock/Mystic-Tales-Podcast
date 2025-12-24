using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.Cache;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.UpdateTrackListenSlot;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ValidateBookingCancellation;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.Detail;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;
using BookingManagementService.BusinessLogic.DTOs.SystemConfiguration;
using BookingManagementService.BusinessLogic.Enums.Account;
using BookingManagementService.BusinessLogic.Enums.Booking;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Enums.Transaction;
using BookingManagementService.BusinessLogic.Helpers.DateHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.DbServices.CachingServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.Infrastructure.Models.Audio.Hls;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Audio.Hls;
using BookingManagementService.Infrastructure.Services.Kafka;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BookingManagementService.BusinessLogic.Services.DbServices.BookingServices
{
    public class BookingProducingRequestService
    {
        private readonly IGenericRepository<BookingProducingRequest> _bookingProducingRequestGenericRepository;
        private readonly IGenericRepository<BookingProducingRequestPodcastTrackToEdit> _bookingProducingRequestPodcastTrackToEditGenericRepository;
        private readonly IGenericRepository<BookingPodcastTrack> _bookingPodcastTrackGenericRepository;
        private readonly IGenericRepository<Booking> _bookingGenericRepository;
        private readonly IGenericRepository<BookingStatusTracking> _bookingStatusTrackingGenericRepository;
        private readonly IGenericRepository<BookingRequirement> _bookingRequirementGenericRepository;
        private readonly IGenericRepository<BookingPodcastTrackListenSession> _bookingPodcastTrackListenSessionGenericRepository;

        private readonly CustomerListenSessionProcedureCachingService _customerListenSessionProcedureCachingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly ILogger<BookingService> _logger;

        private readonly AppDbContext _appDbContext;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;

        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        public BookingProducingRequestService(
            IGenericRepository<BookingProducingRequest> bookingProducingRequestGenericRepository,
            IGenericRepository<BookingProducingRequestPodcastTrackToEdit> bookingProducingRequestPodcastTrackToEditGenericRepository,
            IGenericRepository<BookingPodcastTrack> bookingPodcastTrackGenericRepository,
            IGenericRepository<Booking> bookingGenericRepository,
            IGenericRepository<BookingStatusTracking> bookingStatusTrackingGenericRepository,
            IGenericRepository<BookingRequirement> bookingRequirementGenericRepository,
            IGenericRepository<BookingPodcastTrackListenSession> bookingPodcastTrackListenSessionGenericRepository,
            HttpServiceQueryClient httpServiceQueryClient,
            CustomerListenSessionProcedureCachingService customerListenSessionProcedureCachingService,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<BookingService> logger,
            AppDbContext appDbContext,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            DateHelper dateHelper,
            FFMpegCoreHlsService ffMpegCoreHlsService
            )
        {
            _bookingProducingRequestGenericRepository = bookingProducingRequestGenericRepository;
            _httpServiceQueryClient = httpServiceQueryClient;
            _bookingProducingRequestPodcastTrackToEditGenericRepository = bookingProducingRequestPodcastTrackToEditGenericRepository;
            _bookingPodcastTrackGenericRepository = bookingPodcastTrackGenericRepository;
            _bookingGenericRepository = bookingGenericRepository;
            _bookingStatusTrackingGenericRepository = bookingStatusTrackingGenericRepository;
            _bookingRequirementGenericRepository = bookingRequirementGenericRepository;
            _bookingPodcastTrackListenSessionGenericRepository = bookingPodcastTrackListenSessionGenericRepository;
            _customerListenSessionProcedureCachingService = customerListenSessionProcedureCachingService;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
            _appDbContext = appDbContext;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _dateHelper = dateHelper;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
        }
        public async Task<BookingProducingRequestDetailResponseDTO?> GetProducingRequestByIdAsync(Guid id, AccountStatusCache account)
        {
            try
            {
                var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdAsync(
                    id,
                    includeFunc: include => include
                        .Include(bpr => bpr.BookingPodcastTracks)
                        .ThenInclude(bpt => bpt.BookingRequirement));

                if (bookingProducingRequest == null)
                    return null;

                var booking = await _bookingGenericRepository.FindByIdAsync(bookingProducingRequest.BookingId,
                    includeFunc: function => function
                    .Include(b => b.BookingProducingRequests));

                bool isLastestProducingRequest = true;
                if (account.RoleId == (int)RoleEnum.Customer && booking.AccountId == account.Id)
                {
                    if (booking.BookingProducingRequests.OrderByDescending(bp => bp.CreatedAt).Where(bp => bp.FinishedAt != null).FirstOrDefault()?.Id != bookingProducingRequest.Id)
                    {
                        isLastestProducingRequest = false;
                    }
                }

                // Fix: Use ToListAsync first, then fetch requirement names in a separate async loop
                var editRequirements = await _bookingProducingRequestPodcastTrackToEditGenericRepository.FindAll()
                    .Where(bp => bp.BookingProducingRequestId.Equals(bookingProducingRequest.Id))
                    .Include(bp => bp.BookingPodcastTrack)
                    .ToListAsync();

                var editRequirementList = new List<BookingEditRequirementListItemResponseDTO>();
                foreach (var bp in editRequirements)
                {
                    var requirement = await _bookingRequirementGenericRepository.FindByIdAsync(bp.BookingPodcastTrack.BookingRequirementId);
                    editRequirementList.Add(new BookingEditRequirementListItemResponseDTO
                    {
                        Id = bp.BookingPodcastTrack.BookingRequirementId,
                        Name = requirement?.Name,
                        BookingPodcastTrack = new BookingPodcastTrackListItemResponseDTO
                        {
                            Id = bp.BookingPodcastTrack.Id,
                            BookingId = bp.BookingPodcastTrack.BookingId,
                            BookingProducingRequestId = bp.BookingPodcastTrack.BookingProducingRequestId,
                            BookingRequirementId = bp.BookingPodcastTrack.BookingRequirementId,
                            AudioFileKey = bp.BookingPodcastTrack.AudioFileKey,
                            AudioFileSize = bp.BookingPodcastTrack.AudioFileSize,
                            AudioLength = bp.BookingPodcastTrack.AudioLength,
                            RemainingPreviewListenSlot = bp.BookingPodcastTrack.RemainingPreviewListenSlot
                        }
                    });
                }

                return new BookingProducingRequestDetailResponseDTO
                {
                    Id = bookingProducingRequest.Id,
                    BookingId = bookingProducingRequest.BookingId,
                    Note = bookingProducingRequest.Note,
                    Deadline = bookingProducingRequest.Deadline,
                    DeadlineDays = bookingProducingRequest.DeadlineDays,
                    IsAccepted = bookingProducingRequest.IsAccepted,
                    FinishedAt = bookingProducingRequest.FinishedAt,
                    RejectReason = bookingProducingRequest.RejectReason,
                    CreatedAt = bookingProducingRequest.CreatedAt,
                    BookingPodcastTracks = isLastestProducingRequest ?
                    bookingProducingRequest.BookingPodcastTracks?
                    .OrderBy(nego => nego.BookingRequirement.Order)
                    .Select(nego => new BookingPodcastTrackWithRequirementListItemResponseDTO
                    {
                        Id = nego.Id,
                        BookingId = nego.BookingId,
                        BookingProducingRequestId = nego.BookingProducingRequestId,
                        BookingRequirement = new DTOs.Snippet.BookingRequirementSnippetResponseDTO
                        {
                            Id = nego.BookingRequirement.Id,
                            Name = nego.BookingRequirement.Name,
                            Description = nego.BookingRequirement.Description,
                            Order = nego.BookingRequirement.Order,
                            WordCount = nego.BookingRequirement.WordCount
                        },
                        AudioFileKey = nego.AudioFileKey,
                        AudioFileSize = nego.AudioFileSize,
                        AudioLength = nego.AudioLength,
                        RemainingPreviewListenSlot = nego.RemainingPreviewListenSlot
                    }).ToList() : new List<BookingPodcastTrackWithRequirementListItemResponseDTO>() ?? new List<BookingPodcastTrackWithRequirementListItemResponseDTO>(),
                    EditRequirementList = editRequirementList
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking producing request with ID: {ProducingRequestId}", id);
                throw new HttpRequestException($"Retrieving Producing Request with Id: {id} failed. Error: {ex.Message}");
            }
        }
        public async Task CreateProducingRequestAsync(CreateProducingRequestParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var isValid = await ValidateBookingAccountAsync(parameter.BookingId, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new HttpRequestException("The logged in account are not authorized to create booking producing request for this booking.");
                    }

                    var previousProducingRequest = await _bookingProducingRequestGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ppr => ppr.BookingPodcastTracks))
                        .Where(pr => pr.BookingId == parameter.BookingId && pr.FinishedAt != null)
                        .OrderByDescending(pr => pr.CreatedAt)
                        .FirstOrDefaultAsync();

                    var newProducingRequest = new BookingProducingRequest
                    {
                        BookingId = parameter.BookingId,
                        Note = parameter.Note,
                        Deadline = null,
                        DeadlineDays = parameter.DeadlineDayCount,
                        IsAccepted = null,
                        FinishedAt = null,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    foreach (var trackToEdit in parameter.BookingPodcastTrackIds)
                    {
                        var newTrackToEdit = new BookingProducingRequestPodcastTrackToEdit
                        {
                            //Id = Guid.NewGuid(),
                            BookingProducingRequestId = newProducingRequest.Id,
                            BookingPodcastTrackId = trackToEdit,
                        };
                        newProducingRequest.BookingProducingRequestPodcastTrackToEdits.Add(newTrackToEdit);
                    }
                    var producingRequest = await _bookingProducingRequestGenericRepository.CreateAsync(newProducingRequest);

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        parameter.BookingId,
                        includeFunc: include => include
                            .Include(b => b.BookingStatusTrackings)
                    );

                    if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.TrackPreviewing)
                    {
                        await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                        {
                            //Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = (int)BookingStatusEnum.ProducingRequested,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        });
                    }
                    else
                    {
                        throw new HttpRequestException("Current booking status is not valid for creating producing request");
                    }

                    if (previousProducingRequest != null)
                    {
                        // Complete all listen sessions in the previous producing request
                        var listenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll()
                        .Where(ls => !ls.IsCompleted && previousProducingRequest.BookingPodcastTracks.Select(bp => bp.Id).Contains(ls.BookingPodcastTrackId) && ls.AccountId == parameter.AccountId)
                        .ToListAsync();
                        foreach (var session in listenSession)
                        {
                            session.IsCompleted = true;
                            await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                        }

                        // Complete all listen session procedures of the previous producing request in cache
                        var customerListenSessionCacheKey = await _customerListenSessionProcedureCachingService.GetAllProceduresByCustomerIdAsync(booking.AccountId);
                        foreach (var procedure in customerListenSessionCacheKey.Values)
                        {
                            if (!procedure.IsCompleted && procedure.SourceDetail.Booking != null)
                            {
                                bool belongsToThisBooking = booking.BookingProducingRequests
                                    .Any(bp => bp.Id == procedure.SourceDetail.Booking.BookingProducingRequestId);

                                if (belongsToThisBooking)
                                {
                                    await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(booking.AccountId, procedure.Id, true);
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "BookingProducingRequestId", producingRequest.Id },
                        { "BookingId" , producingRequest.BookingId},
                        { "Note", producingRequest.Note},
                        { "DeadlineDays", producingRequest.DeadlineDays },
                        { "BookingPodcastTrackIds", JArray.FromObject(producingRequest.BookingProducingRequestPodcastTrackToEdits.Select(x => x.BookingPodcastTrackId).ToList()) },
                        { "CreatedAt", producingRequest.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Booking producing request create successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating booking producing request for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                        {
                            { "ErrorMessage", "Booking producing request created failed, error: " + ex.Message}
                        };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking producing request create failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task CancelProducingRequestAsync(CancelBookingProducingRequestParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var isValid = await ValidateBookingAccountOrPodcasterAsync(parameter.BookingId, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new Exception("The logged in account are not authorized to request cancel producing request for this booking.");
                    }

                    var booking = await _bookingGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings))
                        .Where(b => b.Id == parameter.BookingId)
                        .FirstOrDefaultAsync();
                    var currentStatus = booking.BookingStatusTrackings.OrderByDescending(bs => bs.CreatedAt).Select(bs => bs.BookingStatusId).FirstOrDefault();
                    if (booking == null)
                    {
                        throw new Exception($"Booking with id {parameter.BookingId} not found");
                    }
                    if (currentStatus < (int)BookingStatusEnum.Producing)
                    {
                        throw new Exception("Booking is not eligible for cancelling");
                    }
                    if (currentStatus == (int)BookingStatusEnum.CustomerCancelledRequest || currentStatus == (int)BookingStatusEnum.PodcastBuddyCancelledRequest)
                    {
                        throw new Exception("Booking is currently in cancelling requested");
                    }
                    if(currentStatus == (int)BookingStatusEnum.CancelledAutomatically || currentStatus == (int)BookingStatusEnum.CancelledManually)
                    {
                        throw new Exception("Booking has been cancelled already");
                    }
                    if(currentStatus == (int)BookingStatusEnum.Completed)
                    {
                        throw new Exception("Booking has been completed already");
                    }
                    var status = 0;
                    if (booking.AccountId == parameter.AccountId)
                    {
                        status = (int)BookingStatusEnum.CustomerCancelledRequest;
                    }
                    if (booking.PodcastBuddyId == parameter.AccountId)
                    {
                        status = (int)BookingStatusEnum.PodcastBuddyCancelledRequest;
                    }

                    var newBookingStatusTracking = new BookingStatusTracking()
                    {
                        BookingId = booking.Id,
                        BookingStatusId = status,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                    booking.BookingManualCancelledReason = parameter.BookingManualCancelledReason;
                    booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                    var previousProducingRequest = await _bookingProducingRequestGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ppr => ppr.BookingPodcastTracks))
                        .Where(pr => pr.BookingId == parameter.BookingId && pr.FinishedAt != null)
                        .OrderByDescending(pr => pr.CreatedAt)
                        .FirstOrDefaultAsync();

                    if(previousProducingRequest != null)
                    {
                        // Complete all listen sessions in the previous producing request
                        var listenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll()
                            .Where(ls => !ls.IsCompleted && previousProducingRequest.BookingPodcastTracks.Select(bp => bp.Id).Contains(ls.BookingPodcastTrackId) && ls.AccountId == parameter.AccountId)
                            .ToListAsync();
                        var Count = listenSession.Count;
                        foreach (var session in listenSession)
                        {
                            session.IsCompleted = true;
                            await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                        }

                        // Complete all listen session procedures of the previous producing request in cache
                        var customerListenSessionCacheKey = await _customerListenSessionProcedureCachingService.GetAllProceduresByCustomerIdAsync(booking.AccountId);
                        foreach (var procedure in customerListenSessionCacheKey.Values)
                        {
                            if (!procedure.IsCompleted && procedure.SourceDetail.Booking != null)
                            {
                                bool belongsToThisBooking = booking.BookingProducingRequests
                                    .Any(bp => bp.Id == procedure.SourceDetail.Booking.BookingProducingRequestId);

                                if (belongsToThisBooking)
                                {
                                    await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(booking.AccountId, procedure.Id, true);
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    newResponseData["UpdatedAt"] = booking.UpdatedAt;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                            topic: KafkaTopicEnum.BookingManagementDomain,
                            requestData: command.RequestData,
                            responseData: newResponseData,
                            sagaInstanceId: sagaId,
                            flowName: flowName,
                            messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Booking producing request cancelled request successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Booking producing request cancelled request failed, error: " + ex.Message}
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Booking producing request cancelled request failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }

        public async Task SubmitBookingTrack(SubmitBookingTrackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var processedFiles = new List<string>();
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var isValid = await ValidateProducingRequestPodcasterAsync(parameter.BookingProducingRequestId, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new HttpRequestException("The logged in account are not authorized to submit tracks for this booking producing request.");
                    }

                    var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdAsync(parameter.BookingProducingRequestId);
                    if (bookingProducingRequest.FinishedAt != null)
                    {
                        throw new HttpRequestException("This producing request has been finished already");
                    }
                    var config = await GetActiveSystemConfigProfile();
                    if(config == null)
                    {
                        throw new HttpRequestException("System configuration not found");
                    }
                    var remainingPreviewListenSlot = config.BookingConfig.PodcastTrackPreviewListenSlot;

                    var existingProducingRequest = await _bookingProducingRequestGenericRepository.FindAll(
                        includeFunc: include => include
                            .Include(bpr => bpr.BookingPodcastTracks))
                        .Where(pr => pr.BookingId == bookingProducingRequest.BookingId &&
                                     pr.Id != bookingProducingRequest.Id && pr.FinishedAt != null)
                        .ToListAsync();

                    Console.WriteLine("Existing Producing Requests Count: " + existingProducingRequest.Count);
                    var needToDuplicateTrack = new List<BookingPodcastTrack>();
                    if (existingProducingRequest != null && existingProducingRequest.Count >= 1)
                    {
                        var previousProducingRequest = existingProducingRequest.OrderByDescending(pr => pr.CreatedAt).FirstOrDefault();
                        needToDuplicateTrack = previousProducingRequest.BookingPodcastTracks
                            .Where(existingTrack => parameter.Tracks
                                .All(newTrack => newTrack.Id != existingTrack.BookingRequirementId))
                            .ToList();
                        foreach (var test in needToDuplicateTrack)
                        {
                            _logger.LogInformation("Track to duplicate: BookingTrackId = {BookingTrackId}, BookingRequirementId = {BookingRequirementId}, AudioFileKey = {AudioFileKey}", test.Id, test.BookingRequirementId, test.AudioFileKey);
                        }
                    }

                    var createdTracks = new List<BookingPodcastTrack>();

                    if(parameter.Tracks == null || parameter.Tracks.Count == 0)
                    {
                        throw new HttpRequestException("No tracks provided for submission.");
                    }

                    // Process each track
                    foreach (var trackInfo in parameter.Tracks)
                    {
                        var newBookingPodcastTrack = new BookingPodcastTrack()
                        {
                            BookingId = bookingProducingRequest.BookingId,
                            BookingProducingRequestId = bookingProducingRequest.Id,
                            BookingRequirementId = trackInfo.Id,
                            AudioFileKey = trackInfo.AudioFileKey,
                            AudioFileSize = trackInfo.AudioFileSize,
                            AudioLength = trackInfo.AudioLength,
                            RemainingPreviewListenSlot = remainingPreviewListenSlot
                        };

                        var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.CreateAsync(newBookingPodcastTrack);

                        var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + bookingProducingRequest.BookingId + "\\" + bookingProducingRequest.Id + "\\" + bookingPodcastTrack.Id;
                        if (trackInfo.AudioFileKey != null && trackInfo.AudioFileKey != "")
                        {
                            var TrackAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(trackInfo.AudioFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(trackInfo.AudioFileKey, TrackAudioFileKey);
                            processedFiles.Add(TrackAudioFileKey);
                            await _fileIOHelper.DeleteFileAsync(trackInfo.AudioFileKey);
                            newBookingPodcastTrack.AudioFileKey = TrackAudioFileKey;

                            //hls file processing start
                            var stream = await _fileIOHelper.GetFileStreamAsync(newBookingPodcastTrack.AudioFileKey);
                            if (stream == null)
                            {
                                throw new Exception("Could not retrieve uploaded file from storage");
                            }

                            HlsProcessingResult hlsResult = await _ffMpegCoreHlsService.ProcessAudioToHlsAsync(stream);
                            if (hlsResult.Success == false)
                            {
                                throw new Exception("HLS processing failed: " + hlsResult.ErrorMessage);
                            }

                            var playlistFolderKey = FilePathHelper.CombinePaths(
                                        folderPath,
                                        "playlist"
                                    );
                            foreach (var segment in hlsResult.GeneratedFiles)
                            {
                                var segmentData = segment.FileContent;
                                await _fileIOHelper.UploadBinaryFileAsync(segmentData, playlistFolderKey, segment.FileName);
                            }

                            await _fileIOHelper.UploadBinaryFileAsync(
                                hlsResult.EncryptionKeyFile.FileContent,
                                playlistFolderKey,
                                hlsResult.EncryptionKeyFile.FileName
                            );

                            newBookingPodcastTrack.AudioEncryptionKeyId = hlsResult.EncryptionKeyId;
                            newBookingPodcastTrack.AudioEncryptionKeyFileKey = FilePathHelper.CombinePaths(
                                playlistFolderKey,
                                hlsResult.EncryptionKeyFile.FileName
                            );
                            // hls file processing end

                            await _bookingPodcastTrackGenericRepository.UpdateAsync(newBookingPodcastTrack.Id, newBookingPodcastTrack);
                        }

                        createdTracks.Add(newBookingPodcastTrack);
                    }
                    foreach (var track in needToDuplicateTrack)
                    {
                        var newBookingPodcastTrack = new BookingPodcastTrack()
                        {
                            BookingId = bookingProducingRequest.BookingId,
                            BookingProducingRequestId = bookingProducingRequest.Id,
                            BookingRequirementId = track.BookingRequirementId,
                            AudioFileKey = track.AudioFileKey,
                            AudioFileSize = track.AudioFileSize,
                            AudioLength = track.AudioLength,
                            RemainingPreviewListenSlot = remainingPreviewListenSlot
                        };
                        var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.CreateAsync(newBookingPodcastTrack);

                        var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + bookingProducingRequest.BookingId + "\\" + bookingProducingRequest.Id + "\\" + bookingPodcastTrack.Id;
                        if (track.AudioFileKey != null && track.AudioFileKey != "")
                        {
                            var TrackAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(track.AudioFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(track.AudioFileKey, TrackAudioFileKey);
                            newBookingPodcastTrack.AudioFileKey = TrackAudioFileKey;
                            newBookingPodcastTrack.AudioEncryptionKeyId = track.AudioEncryptionKeyId;
                            newBookingPodcastTrack.AudioEncryptionKeyFileKey = FilePathHelper.CombinePaths(
                                folderPath,
                                "playlist",
                                FilePathHelper.GetFileName(track.AudioEncryptionKeyFileKey)
                            );

                            // copy hls playlist folder to new track(copy folder playlist của track cũ sang track mới)
                            await _fileIOHelper.CopyFolderToFolderAsync(
                                sourceFolderPath: FilePathHelper.CombinePaths(
                                    _filePathConfig.BOOKING_FILE_PATH,
                                    bookingProducingRequest.BookingId.ToString(),
                                    track.BookingProducingRequestId.ToString(),
                                    track.Id.ToString(),
                                    "playlist"
                                ),
                                destinationFolderPath: FilePathHelper.CombinePaths(
                                    _filePathConfig.BOOKING_FILE_PATH,
                                    bookingProducingRequest.BookingId.ToString(),
                                    bookingProducingRequest.Id.ToString(),
                                    bookingPodcastTrack.Id.ToString(),
                                    "playlist"
                                )
                            );

                            await _bookingPodcastTrackGenericRepository.UpdateAsync(newBookingPodcastTrack.Id, newBookingPodcastTrack);
                        }
                    }

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingProducingRequest.BookingId,
                        includeFunc: include => include
                            .Include(b => b.BookingStatusTrackings)
                    );

                    if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.Producing)
                    {
                        await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                        {
                            //Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = (int)BookingStatusEnum.TrackPreviewing,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        });
                    }
                    else
                    {
                        throw new HttpRequestException("Current booking status is not valid for submitting podcast track");
                    }

                    bookingProducingRequest.FinishedAt = _dateHelper.GetNowByAppTimeZone();
                    await _bookingProducingRequestGenericRepository.UpdateAsync(bookingProducingRequest.Id, bookingProducingRequest);

                    await transaction.CommitAsync();

                    if (createdTracks.Any())
                    {
                        var trackSubmissionResults = createdTracks.Select(track => new JObject
                        {
                            { "BookingPodcastTrackId", track.Id },
                            { "AudioFileKey", track.AudioFileKey },
                            { "AudioFileSize", track.AudioFileSize },
                            { "AudioFileLength", track.AudioLength }
                        }).ToArray();

                        var newResponseData = new JObject
                        {
                            { "BookingProducingRequestId", bookingProducingRequest.Id },
                            { "Tracks", JArray.FromObject(trackSubmissionResults) },
                            { "CreatedAt", _dateHelper.GetNowByAppTimeZone() }
                        };
                        var newMessageName = messageName + ".success";
                        var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                            topic: KafkaTopicEnum.BookingManagementDomain,
                            requestData: command.RequestData,
                            responseData: newResponseData,
                            sagaInstanceId: sagaId,
                            flowName: flowName,
                            messageName: newMessageName);
                        var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, sagaId.ToString());
                        _logger.LogInformation("Booking tracks submitted successfully for SagaId: {SagaId}. Total tracks: {TrackCount}", command.SagaInstanceId, createdTracks.Count);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

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

                    foreach (var trackInfo in parameter.Tracks)
                    {
                        if (trackInfo.AudioFileKey != null && trackInfo.AudioFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(trackInfo.AudioFileKey);
                        }
                    }

                    _logger.LogError(ex, "Error occurred while submitting booking tracks for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                        {
                            { "ErrorMessage", "Submit booking tracks failed, error: " + ex.Message}
                        };
                    var newMessageName = command.MessageName + ".failed";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking tracks submit failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task AgreementProducingRequest(AgreeProducingRequestParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var isValid = await ValidateProducingRequestPodcasterAsync(parameter.BookingProducingRequestId, parameter.AccountId);
                    if (!isValid)
                    {
                        throw new HttpRequestException("The logged in account are not authorized to accept or reject this booking producing request.");
                    }

                    var bookingProducingRequestId = parameter.BookingProducingRequestId;
                    var isAccepted = parameter.IsAccepted;

                    var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdAsync(bookingProducingRequestId);
                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        bookingProducingRequest.BookingId,
                        includeFunc: include => include
                            .Include(b => b.BookingStatusTrackings)
                    );

                    bookingProducingRequest.IsAccepted = parameter.IsAccepted;
                    bookingProducingRequest = await _bookingProducingRequestGenericRepository.UpdateAsync(bookingProducingRequest.Id, bookingProducingRequest);


                    if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.ProducingRequested)
                    {
                        if (isAccepted == true)
                        {
                            await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                            {
                                //Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                BookingStatusId = (int)BookingStatusEnum.Producing,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            });

                            bookingProducingRequest.Deadline = _dateHelper.GetNowByAppTimeZone().AddDays((double)bookingProducingRequest.DeadlineDays);
                            bookingProducingRequest = await _bookingProducingRequestGenericRepository.UpdateAsync(bookingProducingRequest.Id, bookingProducingRequest);

                            booking.Deadline = DateOnly.FromDateTime(bookingProducingRequest.Deadline.Value);
                            await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                        }
                        else
                        {
                            if(parameter.RejectReason == null || parameter.RejectReason == "")
                            {
                                throw new Exception("Reject reason is required when rejecting producing request");
                            }
                            bookingProducingRequest.RejectReason = parameter.RejectReason;
                            await _bookingProducingRequestGenericRepository.UpdateAsync(bookingProducingRequest.Id, bookingProducingRequest);
                            await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                            {
                                //Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                BookingStatusId = (int)BookingStatusEnum.TrackPreviewing,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            });
                        }
                    }
                    else
                    {
                        throw new Exception("Current booking status is not valid for agreeing to producing");
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, sagaId.ToString());
                    _logger.LogInformation("Booking producing request agreement successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Booking producing request agreement failed, error: " + ex.Message}
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Booking producing request agreement failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task UpdateBookingPodcastTrackPreviewListenSlot(UpdateTrackListenSlotParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var bookingPodcastTrackId = parameter.BookingPodcastTrackId;
                    var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindByIdAsync(bookingPodcastTrackId);
                    if (bookingPodcastTrack == null)
                    {
                        throw new Exception("Booking podcast track not found");
                    }
                    if (bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
                    {
                        throw new Exception("No remaining preview listen slot");
                    }
                    bookingPodcastTrack.RemainingPreviewListenSlot -= 1;
                    await _bookingPodcastTrackGenericRepository.UpdateAsync(bookingPodcastTrack.Id, bookingPodcastTrack);
                    await transaction.CommitAsync();

                    var newMessageName = messageName + ".success";
                    var newResponseData = new JObject
                    {
                        { "BookingPodcastTrackId", bookingPodcastTrack.Id },
                        { "RemainingPreviewListenSlot", bookingPodcastTrack.RemainingPreviewListenSlot }
                    };
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, sagaId.ToString());
                    _logger.LogInformation("Booking podcast track preview listen update successfully for SagaId: {SagaId}", command.SagaInstanceId);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Booking podcast track preview listen update failed, error: " + ex.Message}
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Booking podcast track preview listen update failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task ValidateProducingRequestCancellationAsync(ValidateBookingCancellationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var systemConfig = await GetActiveSystemConfigProfile();
                    if(systemConfig == null)
                    {
                        throw new Exception("System configuration not found");
                    }
                    var profitRate = systemConfig.BookingConfig.ProfitRate;
                    Console.WriteLine("Profit Rate: = " + profitRate);
                    var depositRate = systemConfig.BookingConfig.DepositRate;
                    Console.WriteLine("Deposit Rate: = " + depositRate);

                    var booking = await _bookingGenericRepository.FindByIdAsync(
                        parameter.BookingId,
                        includeFunc: include => include
                            .Include(b => b.BookingStatusTrackings)
                    );
                    if (booking == null)
                    {
                        throw new Exception("Booking not found");
                    }
                    if (booking.AssignedStaffId != parameter.AccountId)
                    {
                        throw new Exception("Account is not assigned staff of this booking");
                    }
                    var depositPrice = booking.Price * (decimal)depositRate;
                    var currentStatusId = booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId;
                    if (parameter.IsAccepted)
                    {
                        if (currentStatusId == (int)BookingStatusEnum.CustomerCancelledRequest)
                        {
                            if (parameter.CustomerBookingCancelDepositRefundRate == null)
                            {
                                throw new Exception("Customer booking cancel deposit refund rate is required");
                            }
                            booking.CustomerBookingCancelDepositRefundRate = (double)parameter.CustomerBookingCancelDepositRefundRate;
                            Console.WriteLine("Customer Booking Cancel Deposit Refund Rate: = " + booking.CustomerBookingCancelDepositRefundRate);
                            booking.PodcastBuddyBookingCancelDepositRefundRate = 100 - (double)parameter.CustomerBookingCancelDepositRefundRate;
                            Console.WriteLine("Podcast Buddy Booking Cancel Deposit Refund Rate: = " + booking.PodcastBuddyBookingCancelDepositRefundRate);
                            booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                            var refundAmount = depositPrice * (decimal)booking.CustomerBookingCancelDepositRefundRate / 100;
                            Console.WriteLine("Refund Amount: = " + refundAmount);
                            var refundMessageName = "booking-refund-flow";
                            var newRefundRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
                                { "Amount", refundAmount },
                                { "AccountId", booking.AccountId },
                                { "PodcasterId", booking.PodcastBuddyId },
                                { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
                            };
                            var startSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: newRefundRequestData,
                                sagaInstanceId: null,
                                messageName: refundMessageName);
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage1, sagaId.ToString());
                            _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", command.SagaInstanceId);


                            var compensationOriginalAmount = depositPrice * (decimal)booking.PodcastBuddyBookingCancelDepositRefundRate / 100;
                            Console.WriteLine("Compensation Original Amount: = " + compensationOriginalAmount);
                            var compensationAmount = compensationOriginalAmount - compensationOriginalAmount * (decimal)profitRate;
                            Console.WriteLine("Compensation Amount: = " + compensationAmount);
                            var compensationMessageName = "booking-deposit-compensation-flow";
                            var newCompensationRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
                                { "Profit", compensationOriginalAmount * (decimal)profitRate },
                                { "Amount", compensationAmount },
                                { "AccountId", booking.AccountId },
                                { "PodcasterId", booking.PodcastBuddyId },
                                { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
                            };
                            var startSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: newCompensationRequestData,
                                sagaInstanceId: null,
                                messageName: compensationMessageName);
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage2, booking.Id.ToString());
                            _logger.LogInformation("Booking deposit compensation message send successfully for BookingId: {BookingId}", booking.Id);

                            var newBookingStatusTracking = new BookingStatusTracking()
                            {
                                BookingId = booking.Id,
                                BookingStatusId = (int)BookingStatusEnum.CancelledManually,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        }
                        if (currentStatusId == (int)BookingStatusEnum.PodcastBuddyCancelledRequest)
                        {
                            if (parameter.PodcastBuddyBookingCancelDepositRefundRate == null)
                            {
                                throw new Exception("Podcast buddy booking cancel deposit refund rate is required");
                            }
                            booking.CustomerBookingCancelDepositRefundRate = 100 - (double)parameter.PodcastBuddyBookingCancelDepositRefundRate;
                            Console.WriteLine("Customer Booking Cancel Deposit Refund Rate: = " + booking.CustomerBookingCancelDepositRefundRate);
                            booking.PodcastBuddyBookingCancelDepositRefundRate = (double)parameter.PodcastBuddyBookingCancelDepositRefundRate;
                            Console.WriteLine("Podcast Buddy Booking Cancel Deposit Refund Rate: = " + booking.PodcastBuddyBookingCancelDepositRefundRate);
                            booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                            var refundAmount = depositPrice * (decimal)booking.CustomerBookingCancelDepositRefundRate / 100;
                            Console.WriteLine("Refund Amount: = " + refundAmount);
                            var refundMessageName = "booking-refund-flow";
                            var newRefundRequestData = new JObject
                                {
                                    { "BookingId", booking.Id },
                                    { "Amount", refundAmount },
                                    { "AccountId", booking.AccountId },
                                    { "PodcasterId", booking.PodcastBuddyId },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
                                };
                            var startSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: newRefundRequestData,
                                sagaInstanceId: null,
                                messageName: refundMessageName);
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage1, sagaId.ToString());
                            _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", command.SagaInstanceId);


                            var compensationOriginalAmount = depositPrice * (decimal)booking.PodcastBuddyBookingCancelDepositRefundRate / 100;
                            Console.WriteLine("Compensation Original Amount: = " + compensationOriginalAmount);
                            var compensationAmount = compensationOriginalAmount - compensationOriginalAmount * (decimal)profitRate;
                            Console.WriteLine("Compensation Amount: = " + compensationAmount);
                            var compensationMessageName = "booking-deposit-compensation-flow";
                            var newCompensationRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
                                { "Profit", compensationOriginalAmount * (decimal)profitRate },
                                { "Amount", compensationAmount },
                                { "AccountId", booking.AccountId },
                                { "PodcasterId", booking.PodcastBuddyId },
                                { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
                            };
                            var startSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: newCompensationRequestData,
                                sagaInstanceId: null,
                                messageName: compensationMessageName);
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage2, booking.Id.ToString());
                            _logger.LogInformation("Booking deposit compensation message send successfully for BookingId: {BookingId}", booking.Id);

                            var newBookingStatusTracking = new BookingStatusTracking()
                            {
                                BookingId = booking.Id,
                                BookingStatusId = (int)BookingStatusEnum.CancelledManually,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        }
                    }
                    else
                    {
                        var previousStatusTracking = booking.BookingStatusTrackings
                            .OrderByDescending(bst => bst.CreatedAt)
                            .Skip(1)
                            .FirstOrDefault();
                        var newBookingStatusTracking = new BookingStatusTracking()
                        {
                            BookingId = booking.Id,
                            BookingStatusId = previousStatusTracking.BookingStatusId,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        booking.BookingManualCancelledReason = null;
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                    }

                    // Complete all listen session of lastest producing request
                    var producingRequest = await _bookingProducingRequestGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(pr => pr.BookingPodcastTracks))
                        .Where(bpr => bpr.BookingId == booking.Id && bpr.FinishedAt != null)
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefaultAsync();

                    if(producingRequest != null)
                    {
                        var listenSession = await _bookingPodcastTrackListenSessionGenericRepository.FindAll()
                        .Where(ls => !ls.IsCompleted && producingRequest.BookingPodcastTracks.Select(bp => bp.Id).Contains(ls.BookingPodcastTrackId) && ls.AccountId == parameter.AccountId)
                        .ToListAsync();
                        foreach (var session in listenSession)
                        {
                            session.IsCompleted = true;
                            await _bookingPodcastTrackListenSessionGenericRepository.UpdateAsync(session.Id, session);
                        }

                        // Complete all customer listen session procedure of the producing request in cache
                        var customerListenSessionCacheKey = await _customerListenSessionProcedureCachingService.GetAllProceduresByCustomerIdAsync(booking.AccountId);
                        foreach (var procedure in customerListenSessionCacheKey.Values)
                        {
                            if (!procedure.IsCompleted && procedure.SourceDetail.Booking != null )
                            {
                                if(procedure.SourceDetail.Booking.BookingProducingRequestId == producingRequest.Id)
                                    await _customerListenSessionProcedureCachingService.MarkProcedureCompletedAsync(booking.AccountId, procedure.Id, true);
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    newResponseData["UpdatedAt"] = booking.UpdatedAt;
                    var newMessageName = messageName + ".success";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, sagaId.ToString());
                    _logger.LogInformation("Booking cancellation validation successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Booking cancellation validation failed, error: " + ex.Message}
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Booking cancellation validation failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task<bool> IsAudioFileOwnedByPodcasterOrAssignedStaffAsync(string audioFileKey, AccountStatusCache account)
        {
            var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll()
                .Include(bpt => bpt.BookingProducingRequest)
                .ThenInclude(bpr => bpr.Booking)
                .Where(bpt => bpt.AudioFileKey == audioFileKey)
                .FirstOrDefaultAsync();
            if (bookingPodcastTrack == null)
            {
                return false;
            }
            var booking = bookingPodcastTrack.BookingProducingRequest.Booking;
            return booking.PodcastBuddyId == account.Id || booking.AssignedStaffId == account.Id;
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
        public async Task<bool> ValidateProducingRequestPodcasterAsync(Guid BookingProducingRequestId, int accountId)
        {
            var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindAll()
                .Include(pr => pr.Booking)
                .Where(pr => pr.Id.Equals(BookingProducingRequestId))
                .FirstOrDefaultAsync();
            if (bookingProducingRequest == null)
                return false;
            if (bookingProducingRequest.Booking.PodcastBuddyId != accountId)
                return false;
            return true;
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
                _logger.LogError(ex, "Error occurred while fetching active system configuration profile.");
                throw new HttpRequestException("Querying active system config failed. error: " + ex.Message);
            }
        }
    }
}
