using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserService.BusinessLogic.Attributes;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.ActivateAccount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountViolationPoint;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.ChangeAccountStatus;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateAccount;
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
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateChannelFavoritedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteChannelFavorited;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteChannelFavoritedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateShowFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateShowFollowedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteShowFollowed;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateEpisodeSaved;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateEpisodeSavedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteEpisodeSaved;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteEpisodeSavedRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountBalanceAmount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountBalanceAmount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterBalanceAmount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountBalanceAmountRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountBalanceAmountRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterBalanceAmountRollback;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountListenSlot;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterListenCount;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedEpisodeDMCARemoveEpisodeForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedEpisodeUnpublishEpisodeForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedShowDMCARemoveShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedShowUnpublishShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesUnpublishShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesDMCARemoveShowForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFavoritedChannelUnpublishChannelForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedEpisodeEpisodeDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedShowShowDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesShowDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFavoritedChannelChannelDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedChannelShowsUnpublishChannelForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedChannelShowsChannelDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedChannelEpisodesUnpublishChannelForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedChannelEpisodesChannelDeletionForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedPodcasterTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedPodcasterShowsTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedPodcasterEpisodesTerminatePodcasterForce;
using UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdateAccountPassword;

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
                        "CustomerGoogleRegistrationNewAccountPassword" => mailInfo.MailObject.ToObject<CustomerGoogleRegistrationNewAccountPasswordMailViewModel>(),
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

        [MessageHandler("update-account-violation-level", SAGA_TOPIC)]
        public async Task HandleUpdateAccountViolationLevelAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var updateAccountViolationLevelParameterDTO = command.RequestData.ToObject<UpdateAccountViolationLevelParameterDTO>();
                    await _accountService.UserViolationLevelUpdate(updateAccountViolationLevelParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-account-violation-level.failed"    // From YAML onFailure.emit
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

        [MessageHandler("create-channel-favorited-rollback", SAGA_TOPIC)]
        public async Task HandleCreateChannelFavoritedRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createChannelFavoritedRollbackParameterDTO = command.RequestData.ToObject<CreateChannelFavoritedRollbackParameterDTO>();
                    await _accountService.CreateChannelFavoritedRollback(createChannelFavoritedRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-channel-favorited-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-favorited", SAGA_TOPIC)]
        public async Task HandleDeleteChannelFavoritedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteChannelFavoritedParameterDTO = command.RequestData.ToObject<DeleteChannelFavoritedParameterDTO>();
                    await _accountService.DeleteChannelFavorited(deleteChannelFavoritedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-favorited.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-favorited-rollback", SAGA_TOPIC)]
        public async Task HandleDeleteChannelFavoritedRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteChannelFavoritedRollbackParameterDTO = command.RequestData.ToObject<DeleteChannelFavoritedRollbackParameterDTO>();
                    await _accountService.DeleteChannelFavoritedRollback(deleteChannelFavoritedRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-favorited-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-show-followed", SAGA_TOPIC)]
        public async Task HandleCreateShowFollowedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createShowFollowedParameterDTO = command.RequestData.ToObject<CreateShowFollowedParameterDTO>();
                    await _accountService.CreateShowFollowed(createShowFollowedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-show-followed.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-show-followed-rollback", SAGA_TOPIC)]
        public async Task HandleCreateShowFollowedRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createShowFollowedRollbackParameterDTO = command.RequestData.ToObject<CreateShowFollowedRollbackParameterDTO>();
                    await _accountService.CreateShowFollowedRollback(createShowFollowedRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-show-followed-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-followed", SAGA_TOPIC)]
        public async Task HandleDeleteShowFollowedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteShowFollowedParameterDTO = command.RequestData.ToObject<DeleteShowFollowedParameterDTO>();
                    await _accountService.DeleteShowFollowed(deleteShowFollowedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-followed.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-followed-rollback", SAGA_TOPIC)]
        public async Task HandleDeleteShowFollowedRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteShowFollowedRollbackParameterDTO = command.RequestData.ToObject<DeleteShowFollowedRollbackParameterDTO>();
                    await _accountService.DeleteShowFollowedRollback(deleteShowFollowedRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-followed-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-episode-saved", SAGA_TOPIC)]
        public async Task HandleCreateEpisodeSavedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createEpisodeSavedParameterDTO = command.RequestData.ToObject<CreateEpisodeSavedParameterDTO>();
                    await _accountService.CreateEpisodeSaved(createEpisodeSavedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-episode-saved.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-episode-saved-rollback", SAGA_TOPIC)]
        public async Task HandleCreateEpisodeSavedRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createEpisodeSavedRollbackParameterDTO = command.RequestData.ToObject<CreateEpisodeSavedRollbackParameterDTO>();
                    await _accountService.CreateEpisodeSavedRollback(createEpisodeSavedRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-episode-saved-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-episode-saved", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeSavedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteEpisodeSavedParameterDTO = command.RequestData.ToObject<DeleteEpisodeSavedParameterDTO>();
                    await _accountService.DeleteEpisodeSaved(deleteEpisodeSavedParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-episode-saved.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-episode-saved-rollback", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeSavedRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteEpisodeSavedRollbackParameterDTO = command.RequestData.ToObject<DeleteEpisodeSavedRollbackParameterDTO>();
                    await _accountService.DeleteEpisodeSavedRollback(deleteEpisodeSavedRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-episode-saved-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("add-account-balance-amount", SAGA_TOPIC)]
        public async Task HandleAddAccountBalanceAmountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var addAccountBalanceAmountParameterDTO = command.RequestData.ToObject<AddAccountBalanceAmountParameterDTO>();
                    await _accountService.AddAccountBalanceAmount(addAccountBalanceAmountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-account-balance-amount.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("add-account-balance-amount-rollback", SAGA_TOPIC)]
        public async Task HandleAddAccountBalanceAmountRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var addAccountBalanceAmountRollbackParameterDTO = command.RequestData.ToObject<AddAccountBalanceAmountRollbackParameterDTO>();
                    await _accountService.AddAccountBalanceAmountRollback(addAccountBalanceAmountRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-account-balance-amount-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("subtract-account-balance-amount", SAGA_TOPIC)]
        public async Task HandleSubtractAccountBalanceAmountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var subtractAccountBalanceAmountParameterDTO = command.RequestData.ToObject<SubtractAccountBalanceAmountParameterDTO>();
                    await _accountService.SubtractAccountBalanceAmount(subtractAccountBalanceAmountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "subtract-account-balance-amount.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("subtract-account-balance-amount-rollback", SAGA_TOPIC)]
        public async Task HandleSubtractAccountBalanceAmountRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var subtractAccountBalanceAmountRollbackParameterDTO = command.RequestData.ToObject<SubtractAccountBalanceAmountRollbackParameterDTO>();
                    await _accountService.SubtractAccountBalanceAmountRollback(subtractAccountBalanceAmountRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "subtract-account-balance-amount-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("add-podcaster-balance-amount", SAGA_TOPIC)]
        public async Task HandleAddPodcasterBalanceAmountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var addPodcasterBalanceAmountParameterDTO = command.RequestData.ToObject<AddPodcasterBalanceAmountParameterDTO>();
                    await _accountService.AddPodcasterBalanceAmount(addPodcasterBalanceAmountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-podcaster-balance-amount.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("add-podcaster-balance-amount-rollback", SAGA_TOPIC)]
        public async Task HandleAddPodcasterBalanceAmountRollbackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var addPodcasterBalanceAmountRollbackParameterDTO = command.RequestData.ToObject<AddPodcasterBalanceAmountRollbackParameterDTO>();
                    await _accountService.AddPodcasterBalanceAmountRollback(addPodcasterBalanceAmountRollbackParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-podcaster-balance-amount-rollback.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("subtract-account-podcast-listen-slot", SAGA_TOPIC)]
        public async Task HandleSubtractAccountListenSlotAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var subtractAccountListenSlotParameterDTO = command.RequestData.ToObject<SubtractAccountListenSlotParameterDTO>();
                    await _accountService.SubtractAccountListenSlot(subtractAccountListenSlotParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "subtract-account-podcast-listen-slot.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("add-podcaster-listen-count", SAGA_TOPIC)]
        public async Task HandleAddPodcasterListenCountAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var addPodcasterListenCountParameterDTO = command.RequestData.ToObject<AddPodcasterListenCountParameterDTO>();
                    await _accountService.AddPodcasterListenCount(addPodcasterListenCountParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "add-podcaster-listen-count.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-episode-dmca-remove-episode-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedEpisodeDMCARemoveEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO = command.RequestData.ToObject<DeleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO>();
                    await _accountService.DeleteAccountSavedEpisodeDMCARemoveEpisodeForce(deleteAccountSavedEpisodeDMCARemoveEpisodeForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-episode-dmca-remove-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-episode-unpublish-episode-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedEpisodeUnpublishEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO = command.RequestData.ToObject<DeleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO>();
                    await _accountService.DeleteAccountSavedEpisodeUnpublishEpisodeForce(deleteAccountSavedEpisodeUnpublishEpisodeForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-episode-unpublish-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-show-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedShowDMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountFollowedShowDMCARemoveShowForceParameterDTO = command.RequestData.ToObject<DeleteAccountFollowedShowDMCARemoveShowForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedShowDMCARemoveShowForce(deleteAccountFollowedShowDMCARemoveShowForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-show-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-show-episodes-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedShowEpisodeDMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO = command.RequestData.ToObject<DeleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO>();
                    await _accountService.DeleteAccountSavedShowEpisodesDMCARemoveShowForce(deleteAccountSavedShowEpisodesDMCARemoveShowForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-show-episodes-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-show-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedShowUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountFollowedShowUnpublishShowForceParameterDTO = command.RequestData.ToObject<DeleteAccountFollowedShowUnpublishShowForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedShowUnpublishShowForce(deleteAccountFollowedShowUnpublishShowForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-show-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-show-episodes-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedShowEpisodesUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO = command.RequestData.ToObject<DeleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO>();
                    await _accountService.DeleteAccountSavedShowEpisodesUnpublishShowForce(deleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-show-episodes-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-favorited-channel-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFavoritedChannelUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountFavoritedChannelUnpublishChannelForceParameterDTO>();
                    await _accountService.DeleteAccountFavoritedChannelUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-favorited-channel-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-channel-shows-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedChannelShowsUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedChannelShowsUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-channel-shows-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-channel-episodes-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedChannelEpisodesUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO>();
                    await _accountService.DeleteAccountSavedChannelEpisodesUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-channel-episodes-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-episode-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedEpisodeEpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountSavedEpisodeEpisodeDeletionForceParameterDTO>();
                    await _accountService.DeleteAccountSavedEpisodeEpisodeDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-episode-episode-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-show-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedShowShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountFollowedShowShowDeletionForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedShowShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-show-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-show-episodes-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedShowEpisodesShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountSavedShowEpisodesShowDeletionForceParameterDTO>();
                    await _accountService.DeleteAccountSavedShowEpisodesShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-show-episodes-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-favorited-channel-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFavoritedChannelChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountFavoritedChannelChannelDeletionForceParameterDTO>();
                    await _accountService.DeleteAccountFavoritedChannelChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-favorited-channel-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-channel-shows-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedChannelShowsChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountFollowedChannelShowsChannelDeletionForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedChannelShowsChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-channel-shows-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-channel-episodes-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedChannelEpisodesChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteAccountSavedChannelEpisodesChannelDeletionForceParameterDTO>();
                    await _accountService.DeleteAccountSavedChannelEpisodesChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-channel-episodes-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-podcaster-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedPodcasterTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO = command.RequestData.ToObject<DeleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedPodcasterTerminatePodcasterForce(deleteAccountFollowedPodcasterTerminatePodcasterForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-podcaster-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-favorited-podcaster-channels-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO = command.RequestData.ToObject<DeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO>();
                    await _accountService.DeleteAccountFavoritedPodcasterChannelsTerminatePodcasterForce(deleteAccountFavoritedPodcasterChannelsTerminatePodcasterForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-favorited-podcaster-channels-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-followed-podcaster-shows-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountFollowedPodcasterShowsTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO = command.RequestData.ToObject<DeleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO>();
                    await _accountService.DeleteAccountFollowedPodcasterShowsTerminatePodcasterForce(deleteAccountFollowedPodcasterShowsTerminatePodcasterForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-followed-podcaster-shows-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-account-saved-podcaster-episodes-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeleteAccountSavedPodcasterEpisodesTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO = command.RequestData.ToObject<DeleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO>();
                    await _accountService.DeleteAccountSavedPodcasterEpisodesTerminatePodcasterForce(deleteAccountSavedPodcasterEpisodesTerminatePodcasterForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-account-saved-podcaster-episodes-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-account-password", SAGA_TOPIC)]
        public async Task HandleUpdateAccountPasswordAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var passwordUpdateInfoDTO = command.RequestData.ToObject<UpdateAccountPasswordParameterDTO>();
                    await _authService.UpdateAccountPassword(passwordUpdateInfoDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-account-password.failed"    // From YAML onFailure.emit
            );
        }
    }
}

