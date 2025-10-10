using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest;
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

namespace BookingManagementService.BusinessLogic.Services.DbServices
{
    public class BookingProducingRequestService
    {
        private readonly IGenericRepository<BookingProducingRequest> _bookingProducingRequestGenericRepository;
        private readonly IGenericRepository<BookingProducingRequestPodcastTrackToEdit> _bookingProducingRequestPodcastTrackToEditGenericRepository;
        private readonly IGenericRepository<BookingPodcastTrack> _bookingPodcastTrackGenericRepository;
        private readonly IGenericRepository<Booking> _bookingGenericRepository;
        private readonly IGenericRepository<BookingStatusTracking> _bookingStatusTrackingGenericRepository;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly ILogger<BookingService> _logger;

        private readonly AppDbContext _appDbContext;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        public BookingProducingRequestService(
            IGenericRepository<BookingProducingRequest> bookingProducingRequestGenericRepository,
            IGenericRepository<BookingProducingRequestPodcastTrackToEdit> bookingProducingRequestPodcastTrackToEditGenericRepository,
            IGenericRepository<BookingPodcastTrack> bookingPodcastTrackGenericRepository,
            IGenericRepository<Booking> bookingGenericRepository,
            IGenericRepository<BookingStatusTracking> bookingStatusTrackingGenericRepository,
            HttpServiceQueryClient httpServiceQueryClient,
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<BookingService> logger,
            AppDbContext appDbContext,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper
            )
        {
            _bookingProducingRequestGenericRepository = bookingProducingRequestGenericRepository;
            _httpServiceQueryClient = httpServiceQueryClient;
            _bookingProducingRequestPodcastTrackToEditGenericRepository = bookingProducingRequestPodcastTrackToEditGenericRepository;
            _bookingPodcastTrackGenericRepository = bookingPodcastTrackGenericRepository;
            _bookingGenericRepository = bookingGenericRepository;
            _bookingStatusTrackingGenericRepository = bookingStatusTrackingGenericRepository;
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
            _appDbContext = appDbContext;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
        }
        public async Task<BookingProducingRequestResponseDTO?> GetProducingRequestByIdAsync(Guid id)
        {
            var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdWithPaths(
                id,
                "BookingPodcastTracks"
                );

            if (bookingProducingRequest == null)
                return null;

            return new BookingProducingRequestResponseDTO
            {
                Id = bookingProducingRequest.Id,
                BookingId = bookingProducingRequest.BookingId,
                Note = bookingProducingRequest.Note,
                Deadline = bookingProducingRequest.Deadline,
                IsAccepted = bookingProducingRequest.IsAccepted,
                FinishedAt = bookingProducingRequest.FinishedAt,
                CreatedAt = bookingProducingRequest.CreatedAt,
                BookingPodcastTracks = bookingProducingRequest.BookingPodcastTracks?.Select(nego => new BookingPodcastTrackListItemResponseDTO
                {
                    Id = nego.Id,
                    BookingId = nego.BookingId,
                    BookingProducingRequestId = nego.BookingProducingRequestId,
                    AudioFileKey = nego.AudioFileKey,
                    AudioFileSize = nego.AudioFileSize,
                    AudioLength = nego.AudioLength,
                    RemainingPreviewListenSlot = nego.RemainingPreviewListenSlot
                }).ToList() ?? new List<BookingPodcastTrackListItemResponseDTO>()
            };
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

                    var newProducingRequest = new BookingProducingRequest
                    {
                        BookingId = parameter.BookingId,
                        Note = parameter.Note,
                        Deadline = parameter.Deadline,
                        IsAccepted = null,
                        FinishedAt = null,
                        CreatedAt = DateTime.UtcNow
                    };
                    foreach (var trackToEdit in parameter.BookingPodcastTrackIds)
                    {
                        var newTrackToEdit = new BookingProducingRequestPodcastTrackToEdit
                        {
                            Id = Guid.NewGuid(),
                            BookingProducingRequestId = newProducingRequest.Id,
                            BookingPodcastTrackId = trackToEdit,
                        };
                        newProducingRequest.BookingProducingRequestPodcastTrackToEdits.Add(newTrackToEdit);
                    }
                    var producingRequest = await _bookingProducingRequestGenericRepository.CreateAsync(newProducingRequest);

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        parameter.BookingId,
                        "BookingStatusTrackings"
                    );

                    if(booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == 6)
                    {
                        await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = 7,
                            CreatedAt = DateTime.Now
                        });
                    } else
                    {
                        throw new Exception("Current booking status is not valid for creating producing request");
                    }

                    if (producingRequest != null)
                    {
                        var newResponseData = new JObject
                        {
                            { "BookingProducingRequestId", producingRequest.Id },
                            { "BookingId" , producingRequest.BookingId},
                            { "Note", producingRequest.Note},
                            { "Deadline", producingRequest.Deadline.ToString("dd-MM-yyyy") },
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
                    else
                    {
                        _logger.LogError("Something Went Wrong");
                    }
                } catch (Exception ex)
                {
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
        public async Task SubmitBookingTrack(SubmitBookingTrackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdAsync(parameter.BookingProducingRequestId);
                    var config = await GetActiveSystemConfigProfile();
                    var remainingPreviewListenSlot = config["BookingConfig"]?.Value<int?>("PodcastTrackPreviewListenSlot");
                    var bookingPodcastTrackId = Guid.NewGuid();

                    var newBookingPodcastTrack = new BookingPodcastTrack()
                    {
                        Id = bookingPodcastTrackId,
                        BookingId = bookingProducingRequest.BookingId,
                        BookingProducingRequestId = bookingProducingRequest.Id,
                        AudioFileKey = null,
                        AudioFileSize = parameter.AudioFileSize,
                        AudioLength = parameter.AudioLength,
                        RemainingPreviewListenSlot = remainingPreviewListenSlot.Value
                    };

                    var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.CreateAsync(newBookingPodcastTrack);

                    var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + bookingProducingRequest.BookingId;
                    if (parameter.AudioFileKey != null && parameter.AudioFileKey != "")
                    {
                        var TrackAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"{bookingPodcastTrackId}_track_audio{FilePathHelper.GetExtension(parameter.AudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(parameter.AudioFileKey, TrackAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(parameter.AudioFileKey);
                        newBookingPodcastTrack.AudioFileKey = TrackAudioFileKey;
                        await _bookingPodcastTrackGenericRepository.UpdateAsync(newBookingPodcastTrack.Id, newBookingPodcastTrack);
                    }

                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingProducingRequest.BookingId,
                        "BookingStatusTrackings"
                    );

                    if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == 5)
                    {
                        await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            BookingStatusId = 6,
                            CreatedAt = DateTime.Now
                        });
                    }
                    else
                    {
                        throw new Exception("Current booking status is not valid for submitting podcast track");
                    }

                    bookingProducingRequest.FinishedAt = DateTime.Now;
                    await _bookingProducingRequestGenericRepository.UpdateAsync(bookingProducingRequest.Id, bookingProducingRequest);

                    await transaction.CommitAsync();

                    if (bookingPodcastTrack != null)
                    {
                        var newResponseData = new JObject
                        {
                            { "BookingProducingRequestId", bookingPodcastTrack.BookingProducingRequestId },
                            { "BookingPodcastTrackId" , bookingPodcastTrack.Id },
                            { "AudioFileKey", bookingPodcastTrack.AudioFileKey},
                            { "AudioFileSize", bookingPodcastTrack.AudioFileSize },
                            { "AudioFileLength", bookingPodcastTrack.AudioLength },
                            { "CreatedAt", DateTime.Now }
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
                        _logger.LogInformation("Booking track submit successfully for SagaId: {SagaId}", command.SagaInstanceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while submitting booking tracks for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject
                        {
                            { "ErrorMessage", "Submit booking track failed, error: " + ex.Message}
                        };
                    var newMessageName = command.MessageName + ".success";
                    var SagaCommandMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.BookingManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    var result = await _messagingService.SendSagaMessageAsync(SagaCommandMessage, command.SagaInstanceId.ToString());
                    _logger.LogError("Booking track submit failed for SagaId: {SagaId}, error: {error}", command.SagaInstanceId, ex.StackTrace);
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

                    var bookingProducingRequestId = parameter.BookingProducingRequestId;
                    var isAccepted = parameter.IsAccepted;

                    var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdAsync(bookingProducingRequestId);
                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingProducingRequest.BookingId,
                        "BookingStatusTrackings"
                    );

                    bookingProducingRequest.IsAccepted = parameter.IsAccepted;

                    if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == 7)
                    {
                        if(isAccepted == true)
                        {
                            await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                            {
                                Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                BookingStatusId = 5,
                                CreatedAt = DateTime.Now
                            });
                        } else
                        {
                            await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                            {
                                Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                BookingStatusId = 6,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        throw new Exception("Current booking status is not valid for agreeing to producing");
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingProducingRequestId", bookingProducingRequestId},
                        { "IsAccepted", isAccepted },
                        { "UpdatedAt", DateTime.Now }
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
                    _logger.LogInformation("Booking producing request agreement successfully for SagaId: {SagaId}", command.SagaInstanceId);
                } catch (Exception ex)
                {
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
        public async Task<JObject> GetActiveSystemConfigProfile()
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
