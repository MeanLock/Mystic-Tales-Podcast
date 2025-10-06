using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserService.BusinessLogic.Attributes;
using UserService.BusinessLogic.Enums.Kafka;
using UserService.BusinessLogic.Services.DbServices.UserServices;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Infrastructure.Models.Kafka;

namespace UserService.BusinessLogic.MessageHandlers
{
    public class AuthMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly AuthMessagingService _authMessagingService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.UserManagementDomain;


        public AuthMessageHandler(
            AuthMessagingService facilityMessagingService,
            IMessagingService messagingService,
            ILogger<AuthMessageHandler> logger) : base(messagingService, logger)
        {
            _authMessagingService = facilityMessagingService;
            _messagingService = messagingService;
        }

        [MessageHandler("ForgotPasswordEvent", "auth-events")]
        public async Task HandleForgotPasswordAsync(string key, string messageJson)
        {
            try
            {
                _logger.LogInformation("Processing ForgotPassword for key: {Key}", key);

                var envelope = DeserializeMessage<MessageEnvelope<ForgotPasswordEvent>>(messageJson);
                var forgotPasswordEvent = envelope?.Data;

                if (forgotPasswordEvent == null)
                {
                    _logger.LogWarning("ForgotPasswordEvent data is null for key: {Key}", key);
                    return;
                }

                // Business logic: Process forgot password
                await _authMessagingService.ForgotPassword(forgotPasswordEvent.Email_Forgot);

                // Example: Send follow-up message after processing
                var notificationEvent = new EmailNotificationEvent
                {
                    To = forgotPasswordEvent.Email_Noti,
                    Subject = "Thông báo từ UserService gửi đến Architecture_1",
                    Message = $"tôi là HUYYYYYYYYYYYYYYYYYYYYY"
                };

                await _messagingService.SendMessageAsync(notificationEvent, null, "notification-events");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FacilityCreatedEvent for key: {Key}", key);
                throw;
            }
        }



        [MessageHandler("create-booking", "booking-management")]
        public async Task HandleCreateBookingAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    _logger.LogInformation("Creating booking for account {AccountId}",
                        command.RequestData["accountId"]);

                    // Extract data from RequestData (JObject)
                    // var booking = await _bookingService.CreateBookingAsync(
                    //     accountId: command.RequestData["accountId"]!.Value<int>(),
                    //     podcastBuddyId: command.RequestData["podcastBuddyId"]!.Value<int>(),
                    //     title: command.RequestData["title"]!.Value<string>()!,
                    //     description: command.RequestData["description"]!.Value<string>()!
                    // );

                    // Return response as JObject
                    return JObject.FromObject(new
                    {
                        // bookingId = booking.Id,
                        // accountId = booking.AccountId,
                        // podcastBuddyId = booking.PodcastBuddyId,
                        // status = "created"
                    });
                },
                responseTopic: SAGA_TOPIC,
                successEmit: "create-booking.success", // From YAML onSuccess.emit
                failedEmit: "create-booking.failed"    // From YAML onFailure.emit
            );
        }



    }

    #region Event DTOs

    public class ForgotPasswordEvent : BaseMessage
    {
        public string Email_Forgot { get; set; } = string.Empty;
        public string Email_Noti { get; set; } = string.Empty;
        public ForgotPasswordEvent()
        {
            MessageType = nameof(ForgotPasswordEvent);
        }
    }

    public class EmailNotificationEvent : BaseMessage
    {
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public EmailNotificationEvent()
        {
            MessageType = nameof(EmailNotificationEvent);
        }
    }
    public class FacilityCreatedEvent : BaseMessage
    {
        public int FacilityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public FacilityCreatedEvent()
        {
            MessageType = nameof(FacilityCreatedEvent);
        }
    }

    public class FacilityUpdatedEvent : BaseMessage
    {
        public int FacilityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public FacilityUpdatedEvent()
        {
            MessageType = nameof(FacilityUpdatedEvent);
        }
    }

    public class FacilityDeletedEvent : BaseMessage
    {
        public int FacilityId { get; set; }

        public FacilityDeletedEvent()
        {
            MessageType = nameof(FacilityDeletedEvent);
        }
    }

    public class FacilityNotificationEvent : BaseMessage
    {
        public int FacilityId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;

        public FacilityNotificationEvent()
        {
            MessageType = nameof(FacilityNotificationEvent);
        }
    }

    public class FacilityAuditEvent : BaseMessage
    {
        public int FacilityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;

        public FacilityAuditEvent()
        {
            MessageType = nameof(FacilityAuditEvent);
        }
    }

    public class FacilityCleanupEvent : BaseMessage
    {
        public int FacilityId { get; set; }
        public string CleanupType { get; set; } = string.Empty;

        public FacilityCleanupEvent()
        {
            MessageType = nameof(FacilityCleanupEvent);
        }
    }

    #endregion
}
