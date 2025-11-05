using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using BookingManagementService.BusinessLogic.DTOs.Account;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Booking.Details;
using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AcceptBookingDealing;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeBookingNegotitation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingDealing;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.TerminateBookingOfPodcaster;
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
using BookingManagementService.BusinessLogic.Services.DbServices.MiscServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.Infrastructure.Configurations.Audio.Hls.interfaces;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
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

        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly AccountCachingService _accountCachingService;
        private readonly ILogger<BookingService> _logger;
        private readonly IHlsConfig _hlsConfig;

        private readonly AppDbContext _appDbContext;
        private readonly IFilePathConfig _filePathConfig;
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
            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            AccountCachingService accountCachingService,
            ILogger<BookingService> logger,
            IHlsConfig hlsConfig,
            AppDbContext appDbContext,
            IFilePathConfig filePathConfig,
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

            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _hlsConfig = hlsConfig;
            _appDbContext = appDbContext;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _dateHelper = dateHelper;
        }
        public Task<List<BookingListItemResponseDTO>> GetAllBookingsAsync()
        {
            var result = _bookingGenericRepository.FindAll(
                includeFunc: function => function.
                    Include(b => b.BookingStatusTrackings)
                    .ThenInclude(bs => bs.BookingStatus))
                .Select(booking => new BookingListItemResponseDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    AccountId = booking.AccountId,
                    PodcastBuddyId = booking.PodcastBuddyId,
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
                }).ToList();
            return Task.FromResult(result);
        }
        public async Task<BookingDetailResponseDTO?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingGenericRepository.FindByIdWithPaths(
                bookingId,
                "BookingRequirements.PodcastBookingTones.PodcastBookingToneCategories",
                "BookingProducingRequests",
                "BookingStatusTrackings.BookingStatus"
            );

            if (booking == null)
                return null;

            return new BookingDetailResponseDTO
            {
                Id = booking.Id,
                Title = booking.Title,
                Description = booking.Description,
                AccountId = booking.AccountId,
                PodcastBuddyId = booking.PodcastBuddyId,
                Price = booking.Price ?? 0,
                Deadline = booking.Deadline ?? default,
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
                        Description = re.PodcastBookingTone.Description,
                        CreatedAt = re.PodcastBookingTone.CreatedAt,
                        DeletedAt = re.PodcastBookingTone.DeletedAt,
                        PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO()
                        {
                            Id = re.PodcastBookingTone.PodcastBookingToneCategory.Id,
                            Name = re.PodcastBookingTone.PodcastBookingToneCategory.Name
                        }
                    }
                }).ToList() ?? new List<BookingRequirementListItemResponseDTO>(),
                BookingProducingRequestList = booking.BookingProducingRequests?.Select(prod => new BookingProducingRequestListItemResponseDTO
                {
                    Id = prod.Id,
                    BookingId = prod.BookingId,
                    Note = prod.Note,
                    Deadline = prod.Deadline,
                    IsAccepted = prod.IsAccepted ?? false,
                    FinishedAt = prod.FinishedAt ?? default,
                    CreatedAt = prod.CreatedAt
                }).ToList() ?? new List<BookingProducingRequestListItemResponseDTO>(),
                CurrentStatus = new BookingStatusResponseDTO
                {
                    Id = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault().BookingStatus.Id,
                    Name = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault().BookingStatus.Name
                }
            };
        }
        public async Task CreateBookingAsync(CreateBookingParameterDTO parameter, SagaCommandMessage command)
        {
            bool test = false;
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var processedFiles = new List<string>();
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var newBooking = new Booking
                    {
                        Title = parameter.Title,
                        Description = parameter.Description,
                        AccountId = parameter.AccountId,
                        PodcastBuddyId = parameter.PodcastBuddyId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var createdBooking = await _bookingGenericRepository.CreateAsync(newBooking);

                    if (createdBooking == null)
                    {
                        throw new InvalidOperationException("Failed to create booking - returned null");
                    }

                    var createdRequirementDocumentList = new List<BookingRequirement>();

                    Console.WriteLine("________________________________________________________");
                    Console.WriteLine(parameter.BookingRequirementInfoList.Count());
                    // Process each track
                    foreach (var requirementDocumentInfo in parameter.BookingRequirementInfoList)
                    {

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
                            throw new InvalidOperationException("Failed to create booking requirement - returned null");
                        }

                        var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + createdBooking.Id;
                        if (requirementDocumentInfo.RequirementDocumentFileKey != null && requirementDocumentInfo.RequirementDocumentFileKey != "")
                        {
                            var requirementDocumentFileKey = FilePathHelper.CombinePaths(folderPath, $"{createdBookingRequirement.Id}_requirement_document");
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
                        Id = Guid.NewGuid(),
                        BookingId = newBooking.Id,
                        BookingStatusId = (int)BookingStatusEnum.QuotationRequest,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    });

                    await transaction.CommitAsync();
                    test = true;
                    var newResponseData = new JObject
                    {
                        { "BookingId", newBooking.Id },
                        { "Title", newBooking.Title },
                        { "Description", newBooking.Description },
                        { "AccountId", newBooking.AccountId },
                        { "PodcastBuddyId", newBooking.PodcastBuddyId },
                        { "CreatedBookingRequirementDocumentList", JArray.FromObject(createdRequirementDocumentList) },
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
                    if (test)
                    {
                        return;
                    }
                    await transaction.RollbackAsync();
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
        public Task<List<BookingListItemResponseDTO>> GetBookingsByPodcasterIdAsync(int podcastBuddyId)
        {
            var result = _bookingGenericRepository.FindAll(
                includeFunc: function => function.
                    Include(b => b.BookingStatusTrackings)
                    .ThenInclude(bs => bs.BookingStatus))
                .Where(b => b.PodcastBuddyId == podcastBuddyId).Select(booking => new BookingListItemResponseDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    AccountId = booking.AccountId,
                    PodcastBuddyId = booking.PodcastBuddyId,
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
                }).ToList();
            return Task.FromResult(result);
        }
        public Task<List<BookingListItemResponseDTO>> GetBookingsByAccountIdAsync(int accountId)
        {
            var result = _bookingGenericRepository.FindAll(
                includeFunc: function => function.
                    Include(b => b.BookingStatusTrackings)
                    .ThenInclude(bs => bs.BookingStatus))
                .Where(b => b.AccountId == accountId).Select(booking => new BookingListItemResponseDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    AccountId = booking.AccountId,
                    PodcastBuddyId = booking.PodcastBuddyId,
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
                }).ToList();
            return Task.FromResult(result);
        }
        public async Task<List<PodcastBookingToneListItemResponseDTO>> GetAllPodcastBookingTonesAsync()
        {
            return await _podcastBookingToneGenericRepository.FindAll(
                includeFunc: function => function
                .Include(pbt => pbt.PodcastBookingToneCategory))
                .Select(pbt => new PodcastBookingToneListItemResponseDTO
                {
                    Id = pbt.Id,
                    Name = pbt.Name,
                    Description = pbt.Description,
                    PodcastBookingToneCategory = new PodcastBookingToneCategoryDetailResponseDTO
                    {
                        Id = pbt.PodcastBookingToneCategory.Id,
                        Name = pbt.PodcastBookingToneCategory.Name
                    },
                    CreatedAt = pbt.CreatedAt,
                    DeletedAt = pbt.DeletedAt,
                }).ToListAsync();
        }
        public async Task<List<BookingRequirementListItemResponseDTO>> GetAllBookingRequirementByBookingIdAsync(int bookingId)
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

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingId,
                        "BookingStatusTrackings"
                    );

                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }

                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        BookingStatusId = (int)BookingStatusEnum.QuotationRejected,
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

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingId,
                        "BookingStatusTrackings",
                        "BookingProducingRequests"
                    );
                    if (booking != null && booking.Price.HasValue)
                    {
                        var newBookingStatusId = 0;
                        if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId < (int)BookingStatusEnum.Producing)
                        {
                            newBookingStatusId = (int)BookingStatusEnum.QuotationCancelled;
                        }
                        else if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.Producing)
                        {
                            if (booking.BookingProducingRequests.Count() == 1 && booking.BookingProducingRequests.Select(b => b.Deadline).FirstOrDefault() <= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()))
                                newBookingStatusId = (int)BookingStatusEnum.CancelledManually;
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
                            var profitRate = systemConfig.BookingConfig.ProfitRate;
                            var depositRate = systemConfig.BookingConfig.DepositRate;

                            if (booking.AccountId == parameter.AccountId)
                            {
                                var Amount = booking.Price - booking.Price * (decimal)profitRate;
                                var refundMessageName = "booking-refund-flow";
                                var newRequestData = new JObject
                                {
                                    { "BookingId", booking.Id },
                                    { "Profit", booking.Price * (decimal)profitRate },
                                    { "Amount", Amount },
                                    { "AccountId", parameter.AccountId },
                                    { "PodcasterId", booking.PodcastBuddyId },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositRefund }
                                };
                                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: command.RequestData,
                                    sagaInstanceId: null,
                                    messageName: refundMessageName);
                                await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, sagaId.ToString());
                                _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", command.SagaInstanceId);
                            }
                            else
                            {
                                var Amount = booking.Price * (decimal)depositRate;
                                var refundMessageName = "booking-refund-flow";
                                var newRequestData = new JObject
                                {
                                    { "BookingId", booking.Id },
                                    { "Profit", null },
                                    { "Amount", Amount },
                                    { "AccountId", parameter.AccountId },
                                    { "PodcasterId", booking.AccountId },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.BookingDepositCompensation }
                                };
                                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: command.RequestData,
                                    sagaInstanceId: null,
                                    messageName: refundMessageName);
                                await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, sagaId.ToString());
                                _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", command.SagaInstanceId);
                            }
                        }

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
                        _logger.LogError("Booking not found or Price is null for SagaId: {SagaId}", command.SagaInstanceId);
                        throw new Exception("Booking not found or invalid pricing information");
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

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingId,
                        "BookingStatusTrackings"
                    );
                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }
                    if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId != (int)BookingStatusEnum.QuotationRequest)
                    {
                        throw new Exception("Cannot deal booking that is not in 'Quotation Request' status.");
                    }

                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
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
                            bookingRequirement.WordCount = reqInfo.WordCount;
                            await _bookingRequirementGenericRepository.UpdateAsync(bookingRequirement.Id, bookingRequirement);
                        }
                    }

                    booking.Price = parameter.Price;
                    booking.Deadline = DateOnly.FromDateTime(parameter.Deadline.Value);
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

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingId,
                        "BookingStatusTrackings"
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
                        Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        BookingStatusId = (int)BookingStatusEnum.Producing,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                    booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    var staffList = await GetStaffList();
                    booking.AssignedStaffId = await GetRandomItemFromJArray(staffList);
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                    var newBookingProducingRequest = new BookingProducingRequest
                    {
                        Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        Note = string.Empty,
                        Deadline = booking.Deadline ?? default,
                        IsAccepted = null,
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

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingId,
                        "BookingStatusTrackings"
                    );

                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {bookingId} not found");
                    }

                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
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
        public async Task BookingDayResponseAllowedChecking()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var systemConfig = await GetActiveSystemConfigProfile();
                    var previewResponseAllowedDays = systemConfig.BookingConfig.PreviewResponseAllowedDays;
                    var producingRequestResponseAllowedDays = systemConfig.BookingConfig.ProducingRequestResponseAllowedDays;
                    var profitRate = systemConfig.BookingConfig.ProfitRate;
                    var depositRate = systemConfig.BookingConfig.DepositRate;


                    var currentDateTime = _dateHelper.GetNowByAppTimeZone();
                    var previewingBookingList = _bookingGenericRepository.FindAll()
                        .Include(b => b.BookingProducingRequests)
                        .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.TrackPreviewing &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt.HasValue &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt!.Value.AddDays(previewResponseAllowedDays) < currentDateTime)
                        .ToList();

                    var producingRequestBookingList = _bookingGenericRepository.FindAll()
                        .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == (int)BookingStatusEnum.ProducingRequested &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().IsAccepted == null &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().CreatedAt.AddDays(producingRequestResponseAllowedDays) < currentDateTime)
                        .Include(b => b.BookingProducingRequests)
                        .ToList();
                    foreach (var booking in previewingBookingList)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingAutoCancelReason = "ExpiredPreview (quá thời hạn preview và pay the rest)";
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                        if (booking.Price.HasValue)
                        {
                            var Amount = booking.Price * (decimal)depositRate - booking.Price * (decimal)profitRate;
                            var compensationMessageName = "booking-deposit-compenstation-flow";
                            var newRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
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
                    foreach (var booking in producingRequestBookingList)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingAutoCancelReason = "Reason: PodcastBuddyNoResponse (không phản hồi producing request)";
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
                    _logger.LogError(ex, "Error occurred while checking booking deadlines");
                }
            }
        }
        private async Task<JArray?> GetStaffList()
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
                                        RoleId = (int)RoleEnum.Staff
                                    },
                                }),
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

            return result.Results?["activeStaffList"] is JArray staffListArray && staffListArray.Count > 0
                ? staffListArray as JArray
                : null;
        }
        private async Task<int> GetRandomItemFromJArray(JArray? array)
        {
            //if (array == null || array.Count == 0)
            //    return null;

            //var random = new Random();
            //var randomIndex = random.Next(array.Count);
            //return array[randomIndex];

            var assignedStaffIds = await _bookingGenericRepository.FindAll()
                .Select(pbrrs => pbrrs.AssignedStaffId)
                .ToListAsync();

            List<AccountDTO> availableStaff = array.ToObject<List<AccountDTO>>();
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
        public async Task TerminateBookingOfPodcasterAsync(TerminateBookingOfPodcasterParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    //var booking = _bookingGenericRepository.FindAll()
                    //    .Include(b => b.BookingStatusTrackings)
                    //    .Where(b => b.PodcastBuddyId == parameter.PodcasterId && 
                    //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 3 &&
                    //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 4 &&
                    //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 8 &&
                    //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 9 &&
                    //    b.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).FirstOrDefault().BookingStatusId != 10 )
                    //    .ToList();
                    var booking = _bookingGenericRepository.FindAll()
                        .Include(b => b.BookingStatusTrackings)
                        .Where(b => b.PodcastBuddyId == parameter.PodcasterId)
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
                                Id = Guid.NewGuid(),
                                BookingId = b.Id,
                                BookingStatusId = (int)BookingStatusEnum.CancelledAutomatically,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            };
                            await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                            b.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            b.BookingAutoCancelReason = "TerminatedByPodcaster (bị hủy bởi podcaster)";
                            await _bookingGenericRepository.UpdateAsync(b.Id, b);
                            if (currentStatus >= (int)BookingStatusEnum.Producing)
                            {
                                var systemConfig = await GetActiveSystemConfigProfile();
                                var profitRate = systemConfig.BookingConfig.ProfitRate;
                                var depositRate = systemConfig.BookingConfig.DepositRate;
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
                    _logger.LogInformation("Booking terminate success for SagaId: {SagaId}", sagaId.ToString());
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while terminating booking for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Terminate booking failed, error: " + ex.Message }
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
                    _logger.LogError("Booking terminate failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        private async Task<SystemConfigProfileDTO?> GetActiveSystemConfigProfile()
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

            var realResult = result.Results?["activeSystemConfigProfile"] is JArray configArray && configArray.Count > 0
                ? configArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<SystemConfigProfileDTO>() : null;
        }

        public async Task<BookingTrackListenResponseDTO> GetTrackListenAsync(int bookingId, Guid podcastTrackId, int accountId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xử lí logic validate + từ lượt nghe + trả BookingTrackListenResponseDTO + gì gì đó 
                    var booking = await _bookingGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(b => b.BookingStatusTrackings)
                        .Include(b => b.BookingProducingRequests))
                        .Where(b => b.Id == bookingId)
                        .FirstOrDefaultAsync();

                    if(booking == null)
                    {
                        throw new Exception("Booking with id " + bookingId + " does not exist");
                    }
                    if (booking.AccountId != accountId)
                    {
                        throw new Exception("You are not authorized to listen to tracks of this booking");
                    }
                    var currentBookingStatusId = booking.BookingStatusTrackings
                        .OrderByDescending(bst => bst.CreatedAt)
                        .FirstOrDefault()
                        .BookingStatusId;
                    if (currentBookingStatusId != (int)BookingStatusEnum.TrackPreviewing)
                    {
                        throw new Exception("Booking with id " + bookingId + " is not in Track Previewing status");
                    }

                    var currentBookingProducingRequest = booking.BookingProducingRequests
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();

                    var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.BookingId == bookingId && bpt.Id == podcastTrackId
                    ).FirstOrDefaultAsync();

                    if (bookingPodcastTrack == null)
                    {
                        throw new Exception("Podcast track with id " + podcastTrackId + " does not exist");
                    }

                    if (currentBookingProducingRequest.Id.Equals(bookingPodcastTrack.BookingProducingRequestId) == false)
                    {
                        throw new Exception("Podcast track with id " + podcastTrackId + " does not belong to the current producing request of booking with id " + bookingId);
                    }

                    if(bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
                    {
                        throw new Exception("You have used up all your preview listen slots for podcast track with id " + podcastTrackId);
                    }

                    bookingPodcastTrack.RemainingPreviewListenSlot -= 1;
                    await _bookingPodcastTrackGenericRepository.UpdateAsync(bookingPodcastTrack.Id, bookingPodcastTrack);

                    var playlistFileKey = FilePathHelper.CombinePaths(
                                        _filePathConfig.BOOKING_FILE_PATH,
                                        currentBookingProducingRequest.Id.ToString(),
                                        bookingPodcastTrack.Id.ToString(),
                                        "playlist",
                                        _hlsConfig.PlaylistFileName
                                    );
                    var result = new BookingTrackListenResponseDTO
                    {
                        PlaylistFileKey = playlistFileKey
                    };
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
                }
            }
        }

        public async Task<byte[]> GetBookingTrackHlsEncryptionKeyFileAsync(int bookingId, Guid podcastTrackId, Guid keyId)
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
                    if (currentBookingStatusId != (int)BookingStatusEnum.TrackPreviewing)
                    {
                        throw new Exception("Booking with id " + bookingId + " is not in Track Previewing status");
                    }

                    var currentBookingProducingRequest = booking.BookingProducingRequests
                        .OrderByDescending(bpr => bpr.CreatedAt)
                        .FirstOrDefault();

                    var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.FindAll(
                        predicate: bpt => bpt.Id == podcastTrackId && bpt.AudioEncryptionKeyId == keyId
                    ).FirstOrDefaultAsync();

                    if (bookingPodcastTrack == null)
                    {
                        throw new Exception("Podcast track with id " + podcastTrackId + " does not exist, or keyId does not match");
                    }

                    if (currentBookingProducingRequest.Id.Equals(bookingPodcastTrack.BookingProducingRequestId) == false)
                    {
                        throw new Exception("Podcast track with id " + podcastTrackId + " does not belong to the current producing request of booking with id " + bookingId);
                    }

                    if (bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
                    {
                        throw new Exception("You have used up all your preview listen slots for podcast track with id " + podcastTrackId);
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

    }
}
