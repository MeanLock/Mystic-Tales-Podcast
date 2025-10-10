using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeBookingNegotitation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities.sqlserver;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace UserService.BusinessLogic.Services.DbServices
{
    public class BookingService
    {
        private readonly IGenericRepository<Booking> _bookingGenericRepository;
        private readonly IGenericRepository<BookingStatusTracking> _bookingStatusTrackingGenericRepository;
        private readonly IGenericRepository<BookingProducingRequest> _bookingProducingRequestGenericRepository;
        private readonly IGenericRepository<BookingNegotiation> _bookingNegotiationGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly ILogger<BookingService> _logger;

        private readonly AppDbContext _appDbContext;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        public BookingService(
            IGenericRepository<Booking> bookingGenericRepository,
            IGenericRepository<BookingStatusTracking> bookingStatusTrackingGenericRepository,
            IGenericRepository<BookingProducingRequest> bookingProducingRequestGenericRepository,
            IGenericRepository<BookingNegotiation> bookingNegotiationGenericRepository,
            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<BookingService> logger,
            AppDbContext appDbContext,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper
            )
        {
            _bookingGenericRepository = bookingGenericRepository;
            _bookingStatusTrackingGenericRepository = bookingStatusTrackingGenericRepository;
            _bookingProducingRequestGenericRepository = bookingProducingRequestGenericRepository;
            _bookingNegotiationGenericRepository = bookingNegotiationGenericRepository;
            _httpServiceQueryClient = httpServiceQueryClient;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
            _appDbContext = appDbContext;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
        }
        public async Task<List<BookingListItemResponseDTO>> GetAllBookingsAsync()
        {
            return _bookingGenericRepository.FindAll().Select(booking => new BookingListItemResponseDTO
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
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _bookingGenericRepository.CreateAsync(newBooking);

                    await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking
                    {
                        Id = Guid.NewGuid(),
                        BookingId = newBooking.Id,
                        BookingStatusId = 1,
                        CreatedAt = DateTime.Now,
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
                    _logger.LogInformation("Booking rejected successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
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
        public async Task<List<BookingListItemResponseDTO>> GetBookingsByAccountIdAsync(int accountId)
        {
            return _bookingGenericRepository.FindAll().Where(b => b.AccountId == accountId).Select(booking => new BookingListItemResponseDTO
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

                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        BookingId = bookingId,
                        BookingStatusId = 3,
                        CreatedAt = DateTime.Now,
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                    booking.UpdatedAt = DateTime.UtcNow;
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

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingId,
                        "BookingStatusTrackings"
                    );
                    if (booking != null)
                    {
                        var newBookingStatusId = 0;
                        if (booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId <= 4)
                        {
                            newBookingStatusId = 4;
                        }
                        else newBookingStatusId = 10;

                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            BookingId = bookingId,
                            BookingStatusId = newBookingStatusId,
                            CreatedAt = DateTime.Now,
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = DateTime.UtcNow;
                        booking.BookingManualCancelledReason = bookingManualCancelledReason;
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);

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
                        _logger.LogError("Something Went Wrong");
                    }
                }
                catch (Exception ex)
                {
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

                    bool isFromCustomer = booking.AccountId == parameter.AccountId ? true : false;

                    var newBookingNegotiation = new BookingNegotiation()
                    {
                        Id = bookingNegotitationId,
                        BookingId = parameter.BookingId,
                        Note = parameter.Note,
                        Deadline = parameter.Deadline,
                        Price = parameter.Price,
                        DemoAudioRequired = parameter.DemoAudioRequired ?? false,
                        DemoAudioFileKey = null,
                        IsCompleted = false,
                        IsFromCustomer = isFromCustomer,
                        CreatedAt = DateTime.Now
                    };
                    await _bookingNegotiationGenericRepository.CreateAsync(newBookingNegotiation);

                    var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + newBookingNegotiation.BookingId;
                    if (parameter.DemoAudioFileKey != null && parameter.DemoAudioFileKey != "")
                    {
                        var DemoAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"{bookingNegotitationId}_negotiation_demo_audio{FilePathHelper.GetExtension(parameter.DemoAudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(parameter.DemoAudioFileKey, DemoAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(parameter.DemoAudioFileKey);
                        newBookingNegotiation.DemoAudioFileKey = DemoAudioFileKey;
                        await _bookingNegotiationGenericRepository.UpdateAsync(newBookingNegotiation.Id, newBookingNegotiation);
                    }

                    _bookingNegotiationGenericRepository.FindAll().Where(bn => bn.BookingId == parameter.BookingId && bn.IsCompleted == false && bn.Id != newBookingNegotiation.Id).ToList().ForEach(async bn =>
                    {
                        bn.IsCompleted = true;
                        await _bookingNegotiationGenericRepository.UpdateAsync(bn.Id, bn);
                    });

                    var currentStatus = booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId;

                    if (currentStatus == 2 && newBookingNegotiation.IsFromCustomer)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            BookingId = booking.Id,
                            BookingStatusId = 1,
                            CreatedAt = DateTime.Now,
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = DateTime.UtcNow;
                        await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                    }
                    else if(currentStatus == 1 && !newBookingNegotiation.IsFromCustomer)
                    {
                        var newBookingStatusTracking = new BookingStatusTracking
                        {
                            BookingId = booking.Id,
                            BookingStatusId = 2,
                            CreatedAt = DateTime.Now,
                        };
                        await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);
                        booking.UpdatedAt = DateTime.UtcNow;
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
                    _logger.LogError(ex, "Error occurred while cancelling booking for SagaId: {SagaId}", command.SagaInstanceId);

                    if (parameter.DemoAudioFileKey != null && parameter.DemoAudioFileKey != "")
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

                    if(booking.BookingStatusTrackings.OrderByDescending(bst => bst.CreatedAt).First().BookingStatusId != 2)
                    {
                        throw new Exception("Cannot agree on negotiation when booking is rejected");
                    }

                    var bookingNegotiation = await _bookingNegotiationGenericRepository.FindByIdAsync(booking.BookingNegotiations.OrderByDescending(bn => bn.CreatedAt).First().Id);

                    if (bookingNegotiation == null)
                    {
                        throw new Exception("Booking negotiation not found");
                    }

                    bookingNegotiation.IsCompleted = true;

                    await _bookingNegotiationGenericRepository.UpdateAsync(bookingNegotiation.Id, bookingNegotiation);

                    booking.Price = bookingNegotiation.Price;
                    booking.Deadline = bookingNegotiation.Deadline;

                    var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + booking.Id;
                    var DemoAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"demo_audio{FilePathHelper.GetExtension(bookingNegotiation.DemoAudioFileKey)}");
                    await _fileIOHelper.CopyFileToFileAsync(bookingNegotiation.DemoAudioFileKey, DemoAudioFileKey);
                    await _fileIOHelper.DeleteFileAsync(bookingNegotiation.DemoAudioFileKey);
                    booking.DemoAudioFileKey = DemoAudioFileKey;

                    booking.UpdatedAt = DateTime.UtcNow;
                    await _bookingGenericRepository.UpdateAsync(booking.Id, booking);
                    var newBookingStatusTracking = new BookingStatusTracking
                    {
                        BookingId = booking.Id,
                        BookingStatusId = 5,
                        CreatedAt = DateTime.Now,
                    };
                    await _bookingStatusTrackingGenericRepository.CreateAsync(newBookingStatusTracking);

                    await _bookingProducingRequestGenericRepository.CreateAsync(new BookingProducingRequest
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        Note = bookingNegotiation.Note,
                        Deadline = booking.Deadline ?? default(DateOnly),
                        IsAccepted = true,
                        FinishedAt = DateTime.Now,
                        CreatedAt = DateTime.Now
                    });

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
        private async Task<JObject> GetActiveSystemConfigProfile()
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

            return ((JArray)result.Results["activeSystemConfigProfile"]).First as JObject;
        }
    }
}
