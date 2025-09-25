using System.Text.Json;
using Microsoft.Extensions.Logging;
using Architecture_1.BusinessLogic.Attributes;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.BusinessLogic.Services.MessagingServices.interfaces;
using Architecture_1.Infrastructure.Models.Kafka;
using Architecture_1.Infrastructure.Services.Google.Email;
using System.Dynamic;
using Architecture_1.BusinessLogic.DTOs.ViewModels.Mail;

namespace Architecture_1.BusinessLogic.MessageHandlers
{
    public class FacilityMessageHandler : BaseMessageHandler
    {
        private readonly FacilityMessagingService _facilityMessagingService;
        private readonly IMessagingService _messagingService;
        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        public FacilityMessageHandler(
            FacilityMessagingService facilityMessagingService,
            IMessagingService messagingService,
            FluentEmailService fluentEmailService,
            ILogger<FacilityMessageHandler> logger) : base(logger)
        {
            _facilityMessagingService = facilityMessagingService;
            _messagingService = messagingService;
            _fluentEmailService = fluentEmailService;
        }

        [MessageHandler("EmailNotificationEvent", "notification-events")]
        public async Task HandleEmailNotificationAsync(string key, string messageJson)
        {
            try
            {
                _logger.LogInformation("Processing FacilityNotificationEvent for key: {Key}", key);
                
                var envelope = DeserializeMessage<MessageEnvelope<EmailNotificationEvent>>(messageJson);
                var notificationEvent = envelope?.Data;

                if (notificationEvent == null) return;

                // Business logic: Send notification to users/admins
                await _fluentEmailService.SendEmail(notificationEvent.To, new KafkaEmailNotificationViewModel
                    {
                        Message = notificationEvent.Message
                    }, "Mail/Notification"
                    , notificationEvent.Subject);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FacilityNotificationEvent for key: {Key}", key);
                throw;
            }
        }

        [MessageHandler("FacilityCreatedEvent", "facility-events")]
        public async Task HandleFacilityCreatedAsync(string key, string messageJson)
        {
            try
            {
                _logger.LogInformation("Processing FacilityCreatedEvent for key: {Key}", key);
                
                Console.WriteLine($"Message JSON: {messageJson}");
                var envelope = DeserializeMessage<MessageEnvelope<FacilityCreatedEvent>>(messageJson);
                var facilityEvent = envelope?.Data;

                if (facilityEvent == null)
                {
                    _logger.LogWarning("FacilityCreatedEvent data is null for key: {Key}", key);
                    return;
                }

                // Business logic: Process facility creation
                await _facilityMessagingService.CreateFacility(facilityEvent);

                // Example: Send follow-up message after processing
                var notificationEvent = new ForgotPasswordEvent
                {
                    Email_Forgot = facilityEvent.Email_Forgot,
                    Email_Noti = facilityEvent.Email_Noti,
                    CorrelationId = facilityEvent.CorrelationId
                };

                await _messagingService.SendMessageAsync(notificationEvent, null, "auth-events");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FacilityCreatedEvent for key: {Key}", key);
                throw;
            }
        }

        [MessageHandler("FacilityUpdatedEvent", "facility-events")]
        public async Task HandleFacilityUpdatedAsync(string key, string messageJson)
        {
            try
            {
                _logger.LogInformation("Processing FacilityUpdatedEvent for key: {Key}", key);
                
                var envelope = DeserializeMessage<MessageEnvelope<FacilityUpdatedEvent>>(messageJson);
                var facilityEvent = envelope?.Data;

                if (facilityEvent == null) return;

                // Business logic: Process facility update
                await _facilityMessagingService.ProcessFacilityUpdatedEventAsync(facilityEvent.FacilityId);

                // Example: Send audit event
                var auditEvent = new FacilityAuditEvent
                {
                    FacilityId = facilityEvent.FacilityId,
                    Action = "Updated",
                    Details = $"Facility '{facilityEvent.Name}' was updated",
                    CorrelationId = facilityEvent.CorrelationId
                };

                await _messagingService.SendMessageAsync(auditEvent, facilityEvent.FacilityId.ToString());

                _logger.LogInformation("FacilityUpdatedEvent processed successfully for facility: {FacilityId}", facilityEvent.FacilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FacilityUpdatedEvent for key: {Key}", key);
                throw;
            }
        }

        [MessageHandler("FacilityDeletedEvent", "facility-events")]
        public async Task HandleFacilityDeletedAsync(string key, string messageJson)
        {
            try
            {
                _logger.LogInformation("Processing FacilityDeletedEvent for key: {Key}", key);
                
                var envelope = DeserializeMessage<MessageEnvelope<FacilityDeletedEvent>>(messageJson);
                var facilityEvent = envelope?.Data;

                if (facilityEvent == null) return;

                // Business logic: Process facility deletion
                await _facilityMessagingService.ProcessFacilityDeletedEventAsync(facilityEvent.FacilityId);

                // Example: Send cleanup event
                var cleanupEvent = new FacilityCleanupEvent
                {
                    FacilityId = facilityEvent.FacilityId,
                    CleanupType = "SoftDelete",
                    CorrelationId = facilityEvent.CorrelationId
                };

                await _messagingService.SendMessageAsync(cleanupEvent, facilityEvent.FacilityId.ToString());

                _logger.LogInformation("FacilityDeletedEvent processed successfully for facility: {FacilityId}", facilityEvent.FacilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FacilityDeletedEvent for key: {Key}", key);
                throw;
            }
        }

        [MessageHandler("FacilityNotificationEvent", "notification-events")]
        public async Task HandleFacilityNotificationAsync(string key, string messageJson)
        {
            try
            {
                _logger.LogInformation("Processing FacilityNotificationEvent for key: {Key}", key);
                
                var envelope = DeserializeMessage<MessageEnvelope<FacilityNotificationEvent>>(messageJson);
                var notificationEvent = envelope?.Data;

                if (notificationEvent == null) return;

                // Business logic: Send notification to users/admins
                _logger.LogInformation("Notification: {Message}", notificationEvent.Message);

                _logger.LogInformation("FacilityNotificationEvent processed successfully for facility: {FacilityId}", notificationEvent.FacilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FacilityNotificationEvent for key: {Key}", key);
                throw;
            }
        }
    }

    #region Event DTOs

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
    
    public class ForgotPasswordEvent : BaseMessage
    {
        public string Email_Forgot { get; set; } = string.Empty;
        public string Email_Noti { get; set; } = string.Empty;
        public ForgotPasswordEvent()
        {
            MessageType = nameof(ForgotPasswordEvent);
        }
    }

    public class FacilityCreatedEvent : BaseMessage
    {

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Email_Forgot { get; set; } = string.Empty;
        public string Email_Noti { get; set; } = string.Empty;

        // public dynamic Facility { get; set; } = new ExpandoObject();

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
