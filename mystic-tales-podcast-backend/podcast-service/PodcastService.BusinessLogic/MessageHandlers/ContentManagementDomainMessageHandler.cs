using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PodcastService.BusinessLogic.Attributes;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateAccount;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountGoogle;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountManual;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendPodcastServiceEmail;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateUser;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.VerifyAccount;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.BusinessLogic.Models.Mail;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Models.Kafka;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.BusinessLogic.MessageHandlers
{
    public class ContentManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly PodcastChannelService _podcastChannelService;
        // private readonly AuthService _authService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ContentManagementDomain;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;



        public ContentManagementDomainMessageHandler(
            IMessagingService messagingService,
            PodcastChannelService podcastChannelService,

            KafkaProducerService kafkaProducerService,
            ILogger<ContentManagementDomainMessageHandler> logger,
            IMailPropertiesConfig mailPropertiesConfig) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _podcastChannelService = podcastChannelService;

            _mailPropertiesConfig = mailPropertiesConfig;
        }

        #region Sample coding format must be followed
        // [MessageHandler("send-user-service-email", SAGA_TOPIC)]
        // public async Task HandleSendPodcastServiceEmailAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var sendPodcastServiceEmailParameterDTO = command.RequestData.ToObject<SendPodcastServiceEmailParameterDTO>();
        //             var mailInfo = sendPodcastServiceEmailParameterDTO.SendPodcastServiceEmailMailInfo;
        //             Console.WriteLine("Preparing to send email of type: " + mailInfo.MailTypeName);
        //             object mailModel = mailInfo.MailTypeName switch
        //             {
        //                 "CustomerRegistrationVerification" => mailInfo.MailObject.ToObject<CustomerRegistrationVerificationMailViewModel>(),
        //                 "CustomerPasswordReset" => mailInfo.MailObject.ToObject<CustomerPasswordResetMailViewModel>(),
        //                 "PodcasterRequestConfirmation" => mailInfo.MailObject.ToObject<PodcasterRequestConfirmationMailViewModel>(),
        //                 "PodcasterRequestResult" => mailInfo.MailObject.ToObject<PodcasterRequestResultMailViewModel>(),
        //                 _ => mailInfo.MailObject.ToObject<object>()
        //             };
        //             Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
        //             var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
        //             await _accountService.SendPodcastServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
        //             // SagaEventMessage KafkaProducerService.PrepareSagaEventMessage(string topic, JObject requestData, JObject responseData, Guid? sagaInstanceId, string flowName, string messageName, [string? key = null])
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: SAGA_TOPIC,
        //                 requestData: command.RequestData,
        //                 responseData: command.RequestData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "send-user-service-email.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "send-user-service-email.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("create-account", SAGA_TOPIC)]
        // public async Task HandleCreateAccountAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var createAccountParameterDTO = command.RequestData.ToObject<CreateAccountParameterDTO>();
        //             await _accountService.RegisterAccount(createAccountParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-account.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("verify-account", SAGA_TOPIC)]
        // public async Task HandleVerifyAccountAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var verifyAccountParameterDTO = command.RequestData.ToObject<VerifyAccountParameterDTO>();
        //             await _authService.AccountVerification(verifyAccountParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "verify-account.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("login-account-manual", SAGA_TOPIC)]
        // public async Task HandleManualLoginAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var manualLoginRequestDTO = command.RequestData.ToObject<LoginAccountManualParameterDTO>();
        //             await _authService.LoginManual(manualLoginRequestDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "login-account-manual.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("login-account-google", SAGA_TOPIC)]
        // public async Task HandleGoogleLoginAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var googleLoginRequestDTO = command.RequestData.ToObject<LoginAccountGoogleParameterDTO>();
        //             await _authService.LoginGoogleAuthorizationCodeFlow(googleLoginRequestDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "login-account-google.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("send-reset-password-link", SAGA_TOPIC)]
        // public async Task HandleSendResetPasswordLinkAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var forgotPasswordParameterDTO = command.RequestData.ToObject<SendResetPasswordLinkParameterDTO>();

        //             await _authService.ForgotPassword(forgotPasswordParameterDTO, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "send-reset-password-link.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("reset-account-password", SAGA_TOPIC)]
        // public async Task HandleResetAccountPasswordAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var resetPasswordRequestDTO = command.RequestData.ToObject<NewResetPasswordParameterDTO>();
        //             await _authService.ResetPassword(resetPasswordRequestDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "reset-account-password.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("change-account-status", SAGA_TOPIC)]
        // public async Task HandleChangeAccountStatusAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var changeAccountStatusParameterDTO = command.RequestData.ToObject<ChangeAccountStatusParameterDTO>();
        //             await _accountService.ChangeAccountStatus(changeAccountStatusParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "change-account-status.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("create-podcaster-profile", SAGA_TOPIC)]
        // public async Task HandleCreatePodcasterProfileAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var createPodcasterProfileParameterDTO = command.RequestData.ToObject<CreatePodcasterProfileParameterDTO>();
        //             await _accountService.CreatePodcasterProfile(createPodcasterProfileParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-podcaster-profile.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("update-podcaster-profile", SAGA_TOPIC)]
        // public async Task HandleUpdatePodcasterProfileAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var updatePodcasterProfileParameterDTO = command.RequestData.ToObject<UpdatePodcasterProfileParameterDTO>();
        //             await _accountService.UpdatePodcasterProfile(updatePodcasterProfileParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "update-podcaster-profile.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("update-user", SAGA_TOPIC)]
        // public async Task HandleUpdateUserAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var updateUserParameterDTO = command.RequestData.ToObject<UpdateUserParameterDTO>();
        //             await _accountService.UpdateUser(updateUserParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "update-user.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("deactivate-account", SAGA_TOPIC)]
        // public async Task HandleDeactivateAccountAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var deactivateAccountParameterDTO = command.RequestData.ToObject<DeactivateAccountParameterDTO>();
        //             await _accountService.DeactivateAccount(deactivateAccountParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "deactivate-account.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("activate-account", SAGA_TOPIC)]
        // public async Task HandleActivateAccountAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var activateAccountParameterDTO = command.RequestData.ToObject<ActivateAccountParameterDTO>();
        //             await _accountService.ActivateAccount(activateAccountParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "activate-account.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("add-account-violation-point", SAGA_TOPIC)]
        // public async Task HandleAddAccountViolationPointAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var addAccountViolationPointParameterDTO = command.RequestData.ToObject<AddAccountViolationPointParameterDTO>();
        //             await _accountService.AddAccountViolationPoint(addAccountViolationPointParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "add-account-violation-point.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("verify-podcaster", SAGA_TOPIC)]
        // public async Task HandleVerifyPodcasterAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var verifyPodcasterParameterDTO = command.RequestData.ToObject<VerifyPodcasterParameterDTO>();
        //             await _accountService.VerifyPodcaster(verifyPodcasterParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "verify-podcaster.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("create-podcaster-followed", SAGA_TOPIC)]
        // public async Task HandleCreatePodcasterFollowedAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var createPodcasterFollowedParameterDTO = command.RequestData.ToObject<CreatePodcasterFollowedParameterDTO>();
        //             await _accountService.CreatePodcasterFollowed(createPodcasterFollowedParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-podcaster-followed.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("delete-podcaster-followed", SAGA_TOPIC)]
        // public async Task HandleDeletePodcasterFollowedAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var deletePodcasterFollowedParameterDTO = command.RequestData.ToObject<DeletePodcasterFollowedParameterDTO>();
        //             await _accountService.DeletePodcasterFollowed(deletePodcasterFollowedParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "delete-podcaster-followed.failed"    // From YAML onFailure.emit
        //     );
        // }
        #endregion


        [MessageHandler("send-user-service-email", SAGA_TOPIC)]
        public async Task HandleSendPodcastServiceEmailAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var sendPodcastServiceEmailParameterDTO = command.RequestData.ToObject<SendPodcastServiceEmailParameterDTO>();
                    var mailInfo = sendPodcastServiceEmailParameterDTO.SendPodcastServiceEmailMailInfo;
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
                    await _accountService.SendPodcastServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
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

        [MessageHandler("create-channel", SAGA_TOPIC)]
        public async Task HandleCreateChannelAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createChannelParameterDTO = command.RequestData.ToObject<CreateChannelParameterDTO>();
                    await _podcastChannelService.CreatePodcastChannel(createChannelParameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-channel.failed"    // From YAML onFailure.emit
            );
        }

    }
}

