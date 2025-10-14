using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeBookingNegotitation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Helpers.DateHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BookingManagementService.BusinessLogic.Services.DbServices
{
    public class BookingService
    {
        private readonly IGenericRepository<Booking> _bookingGenericRepository;
        private readonly IGenericRepository<BookingStatusTracking> _bookingStatusTrackingGenericRepository;
        private readonly IGenericRepository<BookingProducingRequest> _bookingProducingRequestGenericRepository;
        private readonly IGenericRepository<BookingNegotiation> _bookingNegotiationGenericRepository;
        private readonly IGenericRepository<BookingChatRoom> _bookingChatRoomGenericRepository;
        private readonly IGenericRepository<BookingChatMember> _bookingChatMemberGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly ILogger<BookingService> _logger;

        private readonly AppDbContext _appDbContext;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;
        public BookingService(
            IGenericRepository<Booking> bookingGenericRepository,
            IGenericRepository<BookingStatusTracking> bookingStatusTrackingGenericRepository,
            IGenericRepository<BookingProducingRequest> bookingProducingRequestGenericRepository,
            IGenericRepository<BookingNegotiation> bookingNegotiationGenericRepository,
            IGenericRepository<BookingChatRoom> bookingChatRoomGenericRepository,
            IGenericRepository<BookingChatMember> bookingChatMemberGenericRepository,
            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<BookingService> logger,
            AppDbContext appDbContext,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            DateHelper dateHelper
            )
        {
            _bookingGenericRepository = bookingGenericRepository;
            _bookingStatusTrackingGenericRepository = bookingStatusTrackingGenericRepository;
            _bookingProducingRequestGenericRepository = bookingProducingRequestGenericRepository;
            _bookingNegotiationGenericRepository = bookingNegotiationGenericRepository;
            _bookingChatRoomGenericRepository = bookingChatRoomGenericRepository;
            _bookingChatMemberGenericRepository = bookingChatMemberGenericRepository;
            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
            _appDbContext = appDbContext;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _dateHelper = dateHelper;
        }
        public Task<List<BookingListItemResponseDTO>> GetAllBookingsAsync()
        {
            var result = _bookingGenericRepository.FindAll().Select(booking => new BookingListItemResponseDTO
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
                UpdatedAt = booking.UpdatedAt
            }).ToList();
            return Task.FromResult(result);
        }
        public async Task<BookingResponseDTO?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingGenericRepository.FindByIdWithPaths(
                bookingId,
                "BookingNegotiations",
                "BookingProducingRequests"
            );

            if (booking == null)
                return null;

            return new BookingResponseDTO
            {
                Id = booking.Id,
                Title = booking.Title,
                Description = booking.Description,
                AccountId = booking.AccountId,
                PodcastBuddyId = booking.PodcastBuddyId,
                Price = booking.Price ?? 0,
                Deadline = booking.Deadline ?? default(DateOnly),
                DemoAudioFileKey = booking.DemoAudioFileKey,
                BookingManualCancelledReason = booking.BookingManualCancelledReason,
                BookingAutoCancelledReason = booking.BookingAutoCancelReason,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                BookingNegotiationList = booking.BookingNegotiations?.Select(nego => new BookingNegotiationListItemResponseDTO
                {
                    Id = nego.Id,
                    BookingId = nego.BookingId,
                    Note = nego.Note,
                    Deadline = nego.Deadline ?? default(DateOnly),
                    Price = nego.Price ?? 0,
                    DemoAudioRequired = nego.DemoAudioRequired,
                    DemoAudioFileKey = nego.DemoAudioFileKey,
                    IsCompleted = nego.IsCompleted,
                    IsFromCustomer = nego.IsFromCustomer,
                    CreatedAt = nego.CreatedAt
                }).ToList() ?? new List<BookingNegotiationListItemResponseDTO>(),
                BookingProducingRequestList = booking.BookingProducingRequests?.Select(prod => new BookingProducingRequestListItemResponseDTO
                {
                    Id = prod.Id,
                    BookingId = prod.BookingId,
                    Note = prod.Note,
                    Deadline = prod.Deadline,
                    IsAccepted = prod.IsAccepted ?? false,
                    FinishedAt = prod.FinishedAt ?? default(DateTime),
                    CreatedAt = prod.CreatedAt
                }).ToList() ?? new List<BookingProducingRequestListItemResponseDTO>()
            };
        }
        public async Task CreateBookingAsync(CreateBookingParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
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

                    await _bookingGenericRepository.CreateAsync(newBooking);

                    await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
                        BookingId = newBooking.Id,
                        BookingStatusId = 1,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    });

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingId", newBooking.Id },
                        { "Title", newBooking.Title },
                        { "Description", newBooking.Description },
                        { "AccountId", newBooking.AccountId },
                        { "PodcastBuddyId", newBooking.PodcastBuddyId },
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
        public Task<List<BookingListItemResponseDTO>> GetBookingsByAccountIdAsync(int accountId)
        {
            var result = _bookingGenericRepository.FindAll().Where(b => b.AccountId == accountId).Select(booking => new BookingListItemResponseDTO
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
                UpdatedAt = booking.UpdatedAt
            }).ToList();
            return Task.FromResult(result);
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
                        BookingStatusId = 3,
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
                        "BookingStatusTrackings"
                    );
                    if (booking != null && booking.Price.HasValue)
                    {
                        var newBookingStatusId = 0;
                        if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId < 5)
                        {
                            newBookingStatusId = 4;
                        }
                        else newBookingStatusId = 10;

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

                        if(newBookingStatusId == 10)
                        {
                            var profitRate = systemConfig?["BookingConfig"]?.Value<double?>("ProfitRate") ?? 0;
                            var depositRate = systemConfig?["BookingConfig"]?.Value<double?>("DepositRate") ?? 0;
                            
                            if(booking.AccountId == parameter.AccountId)
                            {
                                var Amount = booking.Price - booking.Price * (decimal)profitRate;
                                var refundMessageName = "booking-refund-flow";
                                var newRequestData = new JObject
                                {
                                    { "BookingId", booking.Id },
                                    { "Amount", Amount },
                                    { "AccountId", parameter.AccountId },
                                    { "PodcasterId", booking.PodcastBuddyId },
                                    { "TransactionTypeId", 4 }
                                };
                                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.BookingManagementDomain,
                                    requestData: command.RequestData,
                                    sagaInstanceId: null,
                                    messageName: refundMessageName);
                                await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, sagaId.ToString());
                                _logger.LogInformation("Booking refund message send successfully for SagaId: {SagaId}", command.SagaInstanceId);
                            } else
                            {
                                var Amount = booking.Price * (decimal)depositRate;
                                var refundMessageName = "booking-refund-flow";
                                var newRequestData = new JObject
                                {
                                    { "BookingId", booking.Id },
                                    { "Amount", Amount },
                                    { "AccountId", parameter.AccountId },
                                    { "PodcasterId", booking.AccountId },
                                    { "TransactionTypeId", 5 }
                                };
                                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.BookingManagementDomain,
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
        public async Task CreateBookingNegotiationAsync(CreateBookingNegotiationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;
                    var bookingNegotitationId = Guid.NewGuid();

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        parameter.BookingId,
                        "BookingStatusTrackings"
                    );

                    if (booking == null)
                    {
                        throw new Exception($"Booking with ID {parameter.BookingId} not found");
                    }

                    if(booking.AccountId != parameter.AccountId && booking.PodcastBuddyId != parameter.AccountId)
                    {
                        throw new Exception("Only customer or podcast buddy can create negotiation");
                    }

                    bool isFromCustomer = booking.AccountId == parameter.AccountId;

                    var newBookingNegotiation = new BookingNegotiation()
                    {
                        Id = bookingNegotitationId,
                        BookingId = parameter.BookingId,
                        Note = parameter.Note ?? string.Empty,
                        Deadline = parameter.Deadline,
                        Price = parameter.Price,
                        DemoAudioRequired = parameter.DemoAudioRequired ?? false,
                        DemoAudioFileKey = null,
                        IsCompleted = false,
                        IsFromCustomer = isFromCustomer,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _bookingNegotiationGenericRepository.CreateAsync(newBookingNegotiation);

                    var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + newBookingNegotiation.BookingId;
                    Console.WriteLine("sadfghjkljhgfdszdgsdgsfbfhdghdgwretgefsdfsdfsd " + parameter.DemoAudioFileKey);
                    Console.WriteLine("cccccccccccccccccccccccccccccccc " + folderPath);
                    if (!string.IsNullOrEmpty(parameter.DemoAudioFileKey) && !isFromCustomer)
                    {
                        Console.WriteLine("dddddddddddddddddddddddddddddddd " + parameter.DemoAudioFileKey);
                        var DemoAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"{bookingNegotitationId}_negotiation_demo_audio{FilePathHelper.GetExtension(parameter.DemoAudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(parameter.DemoAudioFileKey, DemoAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(parameter.DemoAudioFileKey);
                        newBookingNegotiation.DemoAudioFileKey = DemoAudioFileKey;
                        await _bookingNegotiationGenericRepository.UpdateAsync(newBookingNegotiation.Id, newBookingNegotiation);
                    }

                    // Fix concurrency issue by handling updates one by one
                    var oldNegotiations = _bookingNegotiationGenericRepository.FindAll()
                        .Where(bn => bn.BookingId == parameter.BookingId && bn.IsCompleted == false && bn.Id != newBookingNegotiation.Id)
                        .ToList();
                    
                    foreach (var bn in oldNegotiations)
                    {
                        bn.IsCompleted = true;
                        await _bookingNegotiationGenericRepository.UpdateAsync(bn.Id, bn);
                    }

                    var currentStatus = booking.BookingStatusTrackings?.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;

                    if (currentStatus == 2 && newBookingNegotiation.IsFromCustomer)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = 1,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                    }
                    else if(currentStatus == 1 && !newBookingNegotiation.IsFromCustomer)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = 2,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                    }
                    else
                    {
                        throw new Exception("Cannot create negotiation in the current booking status");
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingNegotiationId", newBookingNegotiation.Id },
                        { "CreatedAt", newBookingNegotiation.CreatedAt }
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
                    _logger.LogInformation("Booking negotiation created successfully for SagaId: {SagaId}", command.SagaInstanceId);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating booking negotiation for SagaId: {SagaId}", command.SagaInstanceId);

                    if (!string.IsNullOrEmpty(parameter.DemoAudioFileKey))
                    {
                        await _fileIOHelper.DeleteFileAsync(parameter.DemoAudioFileKey);
                    }

                    var newResponseData = new JObject
                        {
                            { "ErrorMessage", "Create booking negotiation failed, error: " + ex.Message },
                        };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking negotiation created failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
                }
            }
        }
        public async Task AgreeBookingNegotiationAsync(AgreeBookingNegotiationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        parameter.BookingId,
                        "BookingNegotiations",
                        "BookingStatusTrackings"
                    );

                    if (booking?.BookingStatusTrackings?.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId != 2)
                    {
                        throw new Exception("Cannot agree on negotiation when booking is rejected");
                    }

                    var latestNegotiation = booking.BookingNegotiations?.OrderByDescending(bn => bn.CreatedAt).FirstOrDefault();
                    if (latestNegotiation == null)
                    {
                        throw new Exception("No negotiations found for this booking");
                    }

                    var bookingNegotiation = await _bookingNegotiationGenericRepository.FindByIdAsync(latestNegotiation.Id);

                    if (bookingNegotiation == null)
                    {
                        throw new Exception("Booking negotiation not found");
                    }

                    bookingNegotiation.IsCompleted = true;

                    await _bookingNegotiationGenericRepository.UpdateAsync(bookingNegotiation.Id, bookingNegotiation);

                    booking.Price = bookingNegotiation.Price;
                    booking.Deadline = bookingNegotiation.Deadline;

                    if (!string.IsNullOrEmpty(bookingNegotiation.DemoAudioFileKey))
                    {
                        var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + booking.Id;
                        var DemoAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"demo_audio{FilePathHelper.GetExtension(bookingNegotiation.DemoAudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(bookingNegotiation.DemoAudioFileKey, DemoAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(bookingNegotiation.DemoAudioFileKey);
                        booking.DemoAudioFileKey = DemoAudioFileKey;
                    }

                    booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        BookingStatusId = 5,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);

                    await _bookingProducingRequestGenericRepository.CreateAsync(new BookingProducingRequest
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        Note = bookingNegotiation.Note,
                        Deadline = booking.Deadline ?? default(DateOnly),
                        IsAccepted = true,
                        FinishedAt = _dateHelper.GetNowByAppTimeZone(),
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    });

                    var chatRoom = await _bookingChatRoomGenericRepository.CreateAsync(new BookingChatRoom
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    });

                    if (chatRoom != null)
                    {
                        await _bookingChatMemberGenericRepository.CreateAsync(new BookingChatMember
                        {
                            ChatRoomId = chatRoom.Id,
                            AccountId = booking.AccountId
                        });
                        await _bookingChatMemberGenericRepository.CreateAsync(new BookingChatMember
                        {
                            ChatRoomId = chatRoom.Id,
                            AccountId = booking.PodcastBuddyId,
                        });
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingId", booking.Id },
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
                    _logger.LogInformation("Booking negotiation agreed successfully for SagaId: {SagaId}", command.SagaInstanceId);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while agreeing booking negotiation for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                    {
                        { "ErrorMessage", "Agree booking negotiation failed, error: " + ex.Message }
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
                    _logger.LogError("Booking negotiation agree failed for SagaId: {SagaId}. Error: {error}", command.SagaInstanceId, ex.StackTrace);
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
                        throw new InvalidOperationException($"Booking with ID {bookingId} not found");
                    }

                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
                        BookingId = bookingId,
                        BookingStatusId = 8,
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
                    var previewResponseAllowedDays = systemConfig?["BookingConfig"]?.Value<int?>("PreviewResponseAllowedDays");
                    var producingRequestResponseAllowedDays = systemConfig?["BookingConfig"]?.Value<int?>("ProducingRequestResponseAllowedDays");
                    var profitRate = systemConfig?["BookingConfig"]?.Value<double?>("ProfitRate") ?? 0;
                    var depositRate = systemConfig?["BookingConfig"]?.Value<double?>("DepositRate") ?? 0;


                    var currentDateTime = _dateHelper.GetNowByAppTimeZone();
                    var previewingBookingList = _bookingGenericRepository.FindAll()
                        .Include(b => b.BookingProducingRequests)
                        .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == 6 &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt.HasValue &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().FinishedAt!.Value.AddDays(previewResponseAllowedDays ?? 0) < currentDateTime)
                        .ToList();

                    var producingRequestBookingList = _bookingGenericRepository.FindAll()
                        .Where(b => b.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId == 7 &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().IsAccepted == null &&
                        b.BookingProducingRequests.OrderByDescending(bpr => bpr.CreatedAt).First().CreatedAt.AddDays(producingRequestResponseAllowedDays ?? 0) < currentDateTime)
                        .Include(b => b.BookingProducingRequests)
                        .ToList();
                    foreach (var booking in previewingBookingList)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = 9,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingAutoCancelReason = "ExpiredPreview (quá thời hạn preview và pay the rest)";
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                        if (booking.Price.HasValue)
                        {
                            var Amount = booking.Price * (decimal)depositRate - booking.Price * (decimal)profitRate;
                            var refundMessageName = "booking-deposit-compenstation-flow";
                            var newRequestData = new JObject
                            {
                                { "BookingId", booking.Id },
                                { "Amount", Amount },
                                { "AccountId", booking.AccountId },
                                { "PodcasterId", booking.PodcastBuddyId },
                                { "TransactionTypeId", 5 }
                            };
                            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.BookingManagementDomain,
                                requestData: newRequestData,
                                sagaInstanceId: null,
                                messageName: refundMessageName);
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
                            BookingStatusId = 9,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        booking.BookingAutoCancelReason = "Reason: PodcastBuddyNoResponse (không phản hồi producing request)";
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

                        var startFirstSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.BookingManagementDomain,
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
                                { "TransactionTypeId", 4 }
                            };
                            var startSecondSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.BookingManagementDomain,
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
        private async Task<JObject?> GetActiveSystemConfigProfile()
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
                                    include = "AccountConfig,AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

            return result.Results?["activeSystemConfigProfile"] is JArray configArray && configArray.Count > 0
                ? configArray.First as JObject
                : null;
        }
    }
}
