using BookingManagementService.BusinessLogic.Attributes;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeBookingNegotitation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.UpdateTrackListenSlot;
using BookingManagementService.BusinessLogic.Services.DbServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.Extensions.Logging;

namespace BookingManagementService.BusinessLogic.MessageHandlers
{
    public class BookingManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly ILogger<BookingManagementDomainMessageHandler> _logger;
        private readonly BookingService _bookingService;
        private readonly BookingProducingRequestService _bookingProducingRequestService;
        public BookingManagementDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<BookingManagementDomainMessageHandler> logger,
            BookingService bookingService,
            BookingProducingRequestService bookingProducingRequestService
            ) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
            _bookingService = bookingService;
            _bookingProducingRequestService = bookingProducingRequestService;
        }
        [MessageHandler("create-booking", "booking-management-domain")]
        public async Task HandleCreateBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CreateBookingParameterDTO>();
                    await _bookingService.CreateBookingAsync(requestData, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "create-booking.failed"
            );
        }
        [MessageHandler("reject-booking", "booking-management-domain")]
        public async Task HandleRejectBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<RejectBookingParameterDTO>();
                    await _bookingService.RejectBookingAsync(requestData, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "reject-booking.failed"
            );
        }
        [MessageHandler("cancel-booking-manual", "booking-management-domain")]
        public async Task HandleCancelBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CancelBookingManualParameterDTO>();
                    await _bookingService.ManualCancelBookingAsync(requestData, command);
                    
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "cancel-booking-manual.failed"
            );
        }
        [MessageHandler("create-booking-negotiation", "booking-management-domain")]
        public async Task HandleCreateBookingNegotiationAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CreateBookingNegotiationParameterDTO>();
                    await _bookingService.CreateBookingNegotiationAsync(requestData, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "create-booking-negotiation.failed"
            );
        }
        [MessageHandler("agree-booking-negotiation", "booking-management-domain")]
        public async Task HandleAgreeBookingNegotiationAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<AgreeBookingNegotiationParameterDTO>();
                    await _bookingService.AgreeBookingNegotiationAsync(requestData, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "agree-booking-negotiation.failed"
            );
        }
        [MessageHandler("create-producing-request", "booking-management-domain")]
        public async Task HandleCreateProducingRequestAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CreateProducingRequestParameterDTO>();
                    await _bookingProducingRequestService.CreateProducingRequestAsync(requestData, command);
                    
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "create-producing-request.failed"
            );
        }
        [MessageHandler("submit-booking-track", "booking-management-domain")]
        public async Task HandleSubmitBookingTrackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<SubmitBookingTrackParameterDTO>();
                    await _bookingProducingRequestService.SubmitBookingTrack(requestData, command);
                    
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "submit-booking-track.failed"
            );
        }
        [MessageHandler("agree-producing-request", "booking-management-domain")]
        public async Task HandleAgreeProducingRequestAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<AgreeProducingRequestParameterDTO>();
                    await _bookingProducingRequestService.AgreementProducingRequest(requestData, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "agree-producing-request.failed"
            );
        }
        [MessageHandler("complete-booking", "booking-management-domain")]
        public async Task HandleCompleteBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var bookingId = command.RequestData.ToObject<CompleteBookingParameterDTO>();
                    await _bookingService.CompleteBookingAsync(bookingId, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "complete-booking.failed"
            );
        }
        [MessageHandler("booking-track-preview-flow", "booking-management-domain")]
        public async Task HandleBookingTrackPreviewFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var bookingPodcastTrackId = command.RequestData.ToObject<UpdateTrackListenSlotParameterDTO>();
                    await _bookingProducingRequestService.UpdateBookingPodcastTrackPreviewListenSlot(bookingPodcastTrackId, command);
                },
                responseTopic: "booking-management-domain",
                failedEmitMessage: "booking-track-preview-flow.failed"
            );
        }
    }
}
