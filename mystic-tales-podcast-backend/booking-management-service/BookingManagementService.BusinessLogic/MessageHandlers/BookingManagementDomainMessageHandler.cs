using BookingManagementService.BusinessLogic.Attributes;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AcceptBookingDealing;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AddPodcastTonesToPodcaster;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeBookingNegotitation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelPodcasterBookingsTerminatePodcasterForce;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateProducingRequest;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingDealing;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.TerminateBookingOfPodcaster;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.UpdateTrackListenSlot;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ValidateBookingCancellation;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.DataAccess.Entities.SqlServer;
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
        private const string SAGA_TOPIC = KafkaTopicEnum.BookingManagementDomain;
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
        [MessageHandler("create-booking", SAGA_TOPIC)]
        public async Task HandleCreateBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CreateBookingParameterDTO>();
                    await _bookingService.CreateBookingAsync(requestData, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-booking.failed"
            );
        }
        [MessageHandler("reject-booking", SAGA_TOPIC)]
        public async Task HandleRejectBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<RejectBookingParameterDTO>();
                    await _bookingService.RejectBookingAsync(requestData, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "reject-booking.failed"
            );
        }
        [MessageHandler("cancel-booking-manual", SAGA_TOPIC)]
        public async Task HandleCancelBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CancelBookingManualParameterDTO>();
                    await _bookingService.ManualCancelBookingAsync(requestData, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-booking-manual.failed"
            );
        }
        //[MessageHandler("create-booking-negotiation", SAGA_TOPIC)]
        //public async Task HandleCreateBookingNegotiationAsync(string key, string messageJson)
        //{
        //    await ExecuteSagaCommandMessageAsync(
        //        messageJson,
        //        async (command) =>
        //        {
        //            var requestData = command.RequestData.ToObject<CreateBookingNegotiationParameterDTO>();
        //            await _bookingService.CreateBookingNegotiationAsync(requestData, command);
        //        },
        //        responseTopic: SAGA_TOPIC,
        //        failedEmitMessage: "create-booking-negotiation.failed"
        //    );
        //}
        //[MessageHandler("agree-booking-negotiation", SAGA_TOPIC)]
        //public async Task HandleAgreeBookingNegotiationAsync(string key, string messageJson)
        //{
        //    await ExecuteSagaCommandMessageAsync(
        //        messageJson,
        //        async (command) =>
        //        {
        //            var requestData = command.RequestData.ToObject<AgreeBookingNegotiationParameterDTO>();
        //            await _bookingService.AgreeBookingNegotiationAsync(requestData, command);
        //        },
        //        responseTopic: SAGA_TOPIC,
        //        failedEmitMessage: "agree-booking-negotiation.failed"
        //    );
        //}
        [MessageHandler("process-booking-dealing", SAGA_TOPIC)]
        public async Task HandleBookingDealingFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<ProcessBookingDealingParameterDTO>();
                    await _bookingService.ProcessBookingDealingAsync(parameter, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "process-booking-dealing.failed"
            );
        }
        [MessageHandler("accept-booking-dealing", SAGA_TOPIC)]
        public async Task HandleAcceptBookingDealingFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<AcceptBookingDealingParameterDTO>();
                    await _bookingService.AcceptBookingDealingAsync(parameter, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "accept-booking-dealing.failed"
            );
        }
        [MessageHandler("cancel-booking-producing-request", SAGA_TOPIC)]
        public async Task HandleCancelBookingProducingRequestAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CancelBookingProducingRequestParameterDTO>();
                    await _bookingProducingRequestService.CancelProducingRequestAsync(requestData, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-booking-producing-request.failed"
            );
        }
        [MessageHandler("create-producing-request", SAGA_TOPIC)]
        public async Task HandleCreateProducingRequestAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<CreateProducingRequestParameterDTO>();
                    await _bookingProducingRequestService.CreateProducingRequestAsync(requestData, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-producing-request.failed"
            );
        }
        [MessageHandler("submit-booking-track", SAGA_TOPIC)]
        public async Task HandleSubmitBookingTrackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<SubmitBookingTrackParameterDTO>();
                    await _bookingProducingRequestService.SubmitBookingTrack(requestData, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "submit-booking-track.failed"
            );
        }
        [MessageHandler("agree-producing-request", SAGA_TOPIC)]
        public async Task HandleAgreeProducingRequestAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<AgreeProducingRequestParameterDTO>();
                    await _bookingProducingRequestService.AgreementProducingRequest(requestData, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "agree-producing-request.failed"
            );
        }
        [MessageHandler("complete-booking", SAGA_TOPIC)]
        public async Task HandleCompleteBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var bookingId = command.RequestData.ToObject<CompleteBookingParameterDTO>();
                    await _bookingService.CompleteBookingAsync(bookingId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "complete-booking.failed"
            );
        }
        [MessageHandler("booking-track-preview-flow", SAGA_TOPIC)]
        public async Task HandleBookingTrackPreviewFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var bookingPodcastTrackId = command.RequestData.ToObject<UpdateTrackListenSlotParameterDTO>();
                    await _bookingProducingRequestService.UpdateBookingPodcastTrackPreviewListenSlot(bookingPodcastTrackId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "booking-track-preview-flow.failed"
            );
        }
        [MessageHandler("terminate-booking-of-podcaster", SAGA_TOPIC)]
        public async Task HandleTerminateBookingOfPodcasterAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<TerminateBookingOfPodcasterParameterDTO>();
                    await _bookingService.TerminateBookingOfPodcasterAsync(parameter, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "terminate-booking-of-podcaster.failed"
            );
        }
        [MessageHandler("validate-booking-cancellation", SAGA_TOPIC)]
        public async Task HandleValidateBookingProducingRequestCancellationAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var requestData = command.RequestData.ToObject<ValidateBookingCancellationParameterDTO>();
                    await _bookingProducingRequestService.ValidateProducingRequestCancellationAsync(requestData, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "validate-booking-cancellation.failed"
            );
        }
        [MessageHandler("cancel-podcaster-bookings-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleCancelPodcasterBookingsTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelPodcasterBookingsTerminatePodcasterForceParameterDTO>();
                    await _bookingService.CancelPodcasterBookingsTerminatePodcasterForceAsync(parameter, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-podcaster-bookings-terminate-podcaster-force.failed"
            );
        }
        [MessageHandler("add-podcast-tones-to-podcaster", SAGA_TOPIC)]
        public async Task HandleAddPodcastTonesToPodcasterAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<AddPodcastTonesToPodcasterParameterDTO>();
                    await _bookingService.AddPodcastTonesToPodcasterAsync(parameter, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-podcast-tones-to-podcaster.failed"
            );
        }
    }
}
