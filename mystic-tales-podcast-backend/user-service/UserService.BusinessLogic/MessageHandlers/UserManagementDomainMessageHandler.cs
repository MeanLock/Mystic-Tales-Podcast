using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserService.BusinessLogic.Attributes;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.ActivateAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountViolationPoint;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.ChangeAccountStatus;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcasterFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcasterProfile;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeactivateAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeletePodcasterFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountGoogle;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountManual;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.NewResetPassword;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendResetPasswordLink;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendUserServiceEmail;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdatePodcasterProfile;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateUser;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyPodcaster;
using UserService.BusinessLogic.Models.Mail;
using UserService.BusinessLogic.Enums.Kafka;
using UserService.BusinessLogic.Services.DbServices.UserServices;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.Infrastructure.Models.Kafka;
using UserService.Infrastructure.Services.Kafka;
using UserService.BusinessLogic.Services.DbServices.MiscServices;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateChannelFavorited;

namespace UserService.BusinessLogic.MessageHandlers
{
    public class UserManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly AccountService _accountService;
        private readonly AuthService _authService;
        private readonly MailOperationService _mailOperationService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.UserManagementDomain;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;



        public UserManagementDomainMessageHandler(
            IMessagingService messagingService,
            AccountService accountService,
            AuthService authService,
            KafkaProducerService kafkaProducerService,
            ILogger<UserManagementDomainMessageHandler> logger,
            MailOperationService mailOperationService,
            IMailPropertiesConfig mailPropertiesConfig) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _accountService = accountService;
            _authService = authService;
            _mailPropertiesConfig = mailPropertiesConfig;
            _mailOperationService = mailOperationService;
        }

        // create-account
        // verify-account
        // login-account-manual
        // login-account-google
        // update-user
        // send-reset-password-link
        // reset-account-password
        // create-podcaster-profile
        // update-podcaster-profile
        // verify-podcaster
        // increase-storage-size
        // decrease-storage-size
        // deactivate-account
        // add-account-violation-point
        // subtract-account-violation-point
        // check-violation-threshold
        // decrease-account-violation-point
        // save-episode-history
        // update-podcaster-rating
        // create-podcaster-followed
        // delete-podcaster-followed
        // add-account-balance-amount
        // subtract-account-balance-amount
        // send-user-service-email
        // add-podcaster-balance-amount
        // subtract-podcaster-balance-amount
        // add-account-balance-amount-rollback
        // subtract-account-balance-amount-rollback
        // add-podcaster-balance-amount-rollback
        // subtract-podcaster-balance-amount-rollback

        [MessageHandler("send-user-service-email", SAGA_TOPIC)]
        public async Task HandleSendUserServiceEmailAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var sendUserServiceEmailParameterDTO = command.RequestData.ToObject<SendUserServiceEmailParameterDTO>();
                    var mailInfo = sendUserServiceEmailParameterDTO.SendUserServiceEmailMailInfo;
                    Console.WriteLine("Preparing to send email of type: " + mailInfo.MailTypeName);
                    object mailModel = mailInfo.MailTypeName switch
                    {
                        "CustomerRegistrationVerification" => mailInfo.MailObject.ToObject<CustomerRegistrationVerificationMailViewModel>(),
                        "CustomerPasswordReset" => mailInfo.MailObject.ToObject<CustomerPasswordResetMailViewModel>(),
                        "PodcasterRequestConfirmation" => mailInfo.MailObject.ToObject<PodcasterRequestConfirmationMailViewModel>(),
                        "PodcasterRequestResult" => mailInfo.MailObject.ToObject<PodcasterRequestResultMailViewModel>(),
                        _ => mailInfo.MailObject.ToObject<object>()
                    };
                    Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
                    var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
                    await _mailOperationService.SendUserServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
                    // SagaEventMessage KafkaProducerService.PrepareSagaEventMessage(string topic, JObject requestData, JObject responseData, Guid? sagaInstanceId, string flowName, string messageName, [string? key = null])
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: SAGA_TOPIC,
                        requestData: command.RequestData,
                        responseData: command.RequestData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "send-user-service-email.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "send-user-service-email.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-account", SAGA_TOPIC)]
        public async Task HandleCreateAccountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createAccountParameterDTO = command.RequestData.ToObject<CreateAccountParameterDTO>();
                    await _accountService.RegisterAccount(createAccountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-account.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("verify-account", SAGA_TOPIC)]
        public async Task HandleVerifyAccountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var verifyAccountParameterDTO = command.RequestData.ToObject<VerifyAccountParameterDTO>();
                    await _authService.AccountVerification(verifyAccountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "verify-account.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("login-account-manual", SAGA_TOPIC)]
        public async Task HandleManualLoginAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var manualLoginRequestDTO = command.RequestData.ToObject<LoginAccountManualParameterDTO>();
                    await _authService.LoginManual(manualLoginRequestDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "login-account-manual.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("login-account-google", SAGA_TOPIC)]
        public async Task HandleGoogleLoginAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var googleLoginRequestDTO = command.RequestData.ToObject<LoginAccountGoogleParameterDTO>();
                    await _authService.LoginGoogleAuthorizationCodeFlow(googleLoginRequestDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "login-account-google.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("send-reset-password-link", SAGA_TOPIC)]
        public async Task HandleSendResetPasswordLinkAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var forgotPasswordParameterDTO = command.RequestData.ToObject<SendResetPasswordLinkParameterDTO>();

                    await _authService.ForgotPassword(forgotPasswordParameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "send-reset-password-link.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("reset-account-password", SAGA_TOPIC)]
        public async Task HandleResetAccountPasswordAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var resetPasswordRequestDTO = command.RequestData.ToObject<NewResetPasswordParameterDTO>();
                    await _authService.ResetPassword(resetPasswordRequestDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "reset-account-password.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("change-account-status", SAGA_TOPIC)]
        public async Task HandleChangeAccountStatusAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var changeAccountStatusParameterDTO = command.RequestData.ToObject<ChangeAccountStatusParameterDTO>();
                    await _accountService.ChangeAccountStatus(changeAccountStatusParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "change-account-status.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-podcaster-profile", SAGA_TOPIC)]
        public async Task HandleCreatePodcasterProfileAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createPodcasterProfileParameterDTO = command.RequestData.ToObject<CreatePodcasterProfileParameterDTO>();
                    await _accountService.CreatePodcasterProfile(createPodcasterProfileParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-podcaster-profile.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-podcaster-profile", SAGA_TOPIC)]
        public async Task HandleUpdatePodcasterProfileAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var updatePodcasterProfileParameterDTO = command.RequestData.ToObject<UpdatePodcasterProfileParameterDTO>();
                    await _accountService.UpdatePodcasterProfile(updatePodcasterProfileParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-podcaster-profile.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-user", SAGA_TOPIC)]
        public async Task HandleUpdateUserAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var updateUserParameterDTO = command.RequestData.ToObject<UpdateUserParameterDTO>();
                    await _accountService.UpdateUser(updateUserParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-user.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("deactivate-account", SAGA_TOPIC)]
        public async Task HandleDeactivateAccountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deactivateAccountParameterDTO = command.RequestData.ToObject<DeactivateAccountParameterDTO>();
                    await _accountService.DeactivateAccount(deactivateAccountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "deactivate-account.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("activate-account", SAGA_TOPIC)]
        public async Task HandleActivateAccountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var activateAccountParameterDTO = command.RequestData.ToObject<ActivateAccountParameterDTO>();
                    await _accountService.ActivateAccount(activateAccountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "activate-account.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("add-account-violation-point", SAGA_TOPIC)]
        public async Task HandleAddAccountViolationPointAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var addAccountViolationPointParameterDTO = command.RequestData.ToObject<AddAccountViolationPointParameterDTO>();
                    await _accountService.AddAccountViolationPoint(addAccountViolationPointParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-account-violation-point.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("verify-podcaster", SAGA_TOPIC)]
        public async Task HandleVerifyPodcasterAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var verifyPodcasterParameterDTO = command.RequestData.ToObject<VerifyPodcasterParameterDTO>();
                    await _accountService.VerifyPodcaster(verifyPodcasterParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "verify-podcaster.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-podcaster-followed", SAGA_TOPIC)]
        public async Task HandleCreatePodcasterFollowedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createPodcasterFollowedParameterDTO = command.RequestData.ToObject<CreatePodcasterFollowedParameterDTO>();
                    await _accountService.CreatePodcasterFollowed(createPodcasterFollowedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-podcaster-followed.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-podcaster-followed", SAGA_TOPIC)]
        public async Task HandleDeletePodcasterFollowedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deletePodcasterFollowedParameterDTO = command.RequestData.ToObject<DeletePodcasterFollowedParameterDTO>();
                    await _accountService.DeletePodcasterFollowed(deletePodcasterFollowedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-podcaster-followed.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-channel-favorited", SAGA_TOPIC)]
        public async Task HandleCreateChannelFavoritedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createChannelFavoritedParameterDTO = command.RequestData.ToObject<CreateChannelFavoritedParameterDTO>();
                    await _accountService.CreateChannelFavorited(createChannelFavoritedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-channel-favorited.failed"    // From YAML onFailure.emit
            );
        }




        // [MessageHandler("ForgotPasswordEvent", "auth-events")]
        // public async Task HandleForgotPasswordAsync(string key, string messageJson)
        // {
        //     try
        //     {
        //         _logger.LogInformation("Processing ForgotPassword for key: {Key}", key);

        //         var envelope = DeserializeMessage<MessageEnvelope<ForgotPasswordEvent>>(messageJson);
        //         var forgotPasswordEvent = envelope?.Data;

        //         if (forgotPasswordEvent == null)
        //         {
        //             _logger.LogWarning("ForgotPasswordEvent data is null for key: {Key}", key);
        //             return;
        //         }

        //         // Business logic: Process forgot password

        //         // Example: Send follow-up message after processing
        //         var notificationEvent = new EmailNotificationEvent
        //         {
        //             To = forgotPasswordEvent.Email_Noti,
        //             Subject = "Thông báo từ UserService gửi đến Architecture_1",
        //             Message = $"tôi là HUYYYYYYYYYYYYYYYYYYYYY"
        //         };

        //         await _messagingService.SendMessageAsync(notificationEvent, null, "notification-events");

        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Failed to handle FacilityCreatedEvent for key: {Key}", key);
        //         throw;
        //     }
        // }



        // [MessageHandler("create-booking", "booking-management")]
        // public async Task HandleCreateBookingAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             _logger.LogInformation("Creating booking for account {AccountId}",
        //                 command.RequestData["accountId"]);

        //             // Extract data from RequestData (JObject)
        //             // var booking = await _bookingService.CreateBookingAsync(
        //             //     accountId: command.RequestData["accountId"]!.Value<int>(),
        //             //     podcastBuddyId: command.RequestData["podcastBuddyId"]!.Value<int>(),
        //             //     title: command.RequestData["title"]!.Value<string>()!,
        //             //     description: command.RequestData["description"]!.Value<string>()!
        //             // );

        //             // Return response as JObject
        //             // return await Task.FromResult(JObject.FromObject(new
        //             // {
        //             //     // bookingId = booking.Id,
        //             //     // accountId = booking.AccountId,
        //             //     // podcastBuddyId = booking.PodcastBuddyId,
        //             //     status = "created"
        //             // }));
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         // successEmit: "create-booking.success", // From YAML onSuccess.emit
        //         failedEmitMessage: "create-booking.failed"    // From YAML onFailure.emit
        //     );
        // }


    }
}

