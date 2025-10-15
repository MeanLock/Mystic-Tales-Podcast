using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.UpdateTrackListenSlot;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest;
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
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        private readonly DateHelper _dateHelper;
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
            FileIOHelper fileIOHelper,
            DateHelper dateHelper
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
            _dateHelper = dateHelper;
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
                        Deadline = DateOnly.FromDateTime(parameter.Deadline),
                        IsAccepted = null,
                        FinishedAt = null,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
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
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        });
                    } else
                    {
                        throw new Exception("Current booking status is not valid for creating producing request");
                    }

                    if (producingRequest != null)
                    {
                        await transaction.CommitAsync();
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
                        await transaction.RollbackAsync();
                        _logger.LogError("Something Went Wrong");
                    }
                } catch (Exception ex)
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

                    var createdTracks = new List<BookingPodcastTrack>();

                    // Process each track
                    foreach (var trackInfo in parameter.Tracks)
                    {

                        var newBookingPodcastTrack = new BookingPodcastTrack()
                        {
                            BookingId = bookingProducingRequest.BookingId,
                            BookingProducingRequestId = bookingProducingRequest.Id,
                            AudioFileKey = trackInfo.AudioFileKey,
                            AudioFileSize = trackInfo.AudioFileSize,
                            AudioLength = trackInfo.AudioLength,
                            RemainingPreviewListenSlot = remainingPreviewListenSlot.Value
                        };

                        var bookingPodcastTrack = await _bookingPodcastTrackGenericRepository.CreateAsync(newBookingPodcastTrack);

                        var folderPath = _filePathConfig.BOOKING_FILE_PATH + "\\" + bookingProducingRequest.BookingId;
                        if (trackInfo.AudioFileKey != null && trackInfo.AudioFileKey != "")
                        {
                            var TrackAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"{bookingPodcastTrack.Id}_track_audio{FilePathHelper.GetExtension(trackInfo.AudioFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(trackInfo.AudioFileKey, TrackAudioFileKey);
                            await _fileIOHelper.DeleteFileAsync(trackInfo.AudioFileKey);
                            newBookingPodcastTrack.AudioFileKey = TrackAudioFileKey;
                            await _bookingPodcastTrackGenericRepository.UpdateAsync(newBookingPodcastTrack.Id, newBookingPodcastTrack);
                        }

                        createdTracks.Add(newBookingPodcastTrack);
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
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        });
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Current booking status is not valid for submitting podcast track");
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

                    var bookingProducingRequestId = parameter.BookingProducingRequestId;
                    var isAccepted = parameter.IsAccepted;

                    var bookingProducingRequest = await _bookingProducingRequestGenericRepository.FindByIdAsync(bookingProducingRequestId);
                    var booking = await _bookingGenericRepository.FindByIdWithPaths(
                        bookingProducingRequest.BookingId,
                        "BookingStatusTrackings"
                    );

                    bookingProducingRequest.IsAccepted = parameter.IsAccepted;
                    bookingProducingRequest = await _bookingProducingRequestGenericRepository.UpdateAsync(bookingProducingRequest.Id, bookingProducingRequest);

                    if (booking.BookingStatusTrackings.OrderByDescending(b => b.CreatedAt).First().BookingStatusId == 7)
                    {
                        if(isAccepted == true)
                        {
                            await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                            {
                                Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                BookingStatusId = 5,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            });
                        } else
                        {
                            await _bookingStatusTrackingGenericRepository.CreateAsync(new BookingStatusTracking()
                            {
                                Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                BookingStatusId = 6,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            });
                        }
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Current booking status is not valid for agreeing to producing");
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "BookingProducingRequestId", bookingProducingRequestId},
                        { "IsAccepted", isAccepted },
                        { "UpdatedAt", _dateHelper.GetNowByAppTimeZone() }
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
                    if(bookingPodcastTrack.RemainingPreviewListenSlot <= 0)
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
