
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Architecture_1.Common.AppConfigurations.App;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.Common.AppConfigurations.FilePath;
using Architecture_1.Common.AppConfigurations.FilePath.interfaces;
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Repositories.interfaces;
using Architecture_1.DataAccess.UOW;
using Newtonsoft.Json.Linq;

namespace Architecture_1.BusinessLogic.Services.DbServices.NhapServices
{
    public class ChatService
    {
        public AppDbContext _context;

        // CONFIG
        private readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;

        // HELPERS
        private readonly FileHelpers _fileHelpers;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;
        // REPOSITORIES
        public IGenericRepository<Chat> _chatRepository;

        // AWS SERVICE
        private readonly AWSS3Service _awsS3Service;

        public ChatService(AppDbContext context,
            IUnitOfWork unitOfWork,
            IGenericRepository<Chat> chatRepository,
            IAppConfig appConfig,
            FileHelpers fileHelpers,
            AWSS3Service awsS3Service,
            IFilePathConfig filePathConfig
            )
        {
            _chatRepository = chatRepository;
            _unitOfWork = unitOfWork;
            _context = context;
            _appConfig = appConfig;
            _fileHelpers = fileHelpers;
            _awsS3Service = awsS3Service;
            _filePathConfig = filePathConfig;
        }

        public async Task<string> GenerateImageUrl(string folderPath, string fileName, string type)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                return await _awsS3Service.GeneratePresignedUrlAsync(folderPath, fileName, type);
            }
            return await _fileHelpers.GetImageUrl(folderPath, fileName, type);
            // return await _awsS3Service.GeneratePresignedUrlAsync(folderPath, fileName, type);
        }

        public async Task SaveBase64File(string base64Data, string folderPath, string fileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.UploadBase64FileAsync(base64Data, folderPath, fileName);
                return;
            }
            await _fileHelpers.SaveBase64File(base64Data, folderPath, fileName);
            // await _awsS3Service.UploadBase64FileAsync(base64Data, folderPath, fileName);
        }

        public async Task CopyFile(string sourceFolder, string sourceFileName, string destinationFolder, string destinationFileName)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.CopyFileAsync(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
                return;
            }
            await _fileHelpers.CopyFile(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
            // await _awsS3Service.CopyFileAsync(sourceFolder, sourceFileName, destinationFolder, destinationFileName);
        }

        public async Task DeleteFolder(string folderPath)
        {
            if (_appConfig.IMAGE_SRC == "awsS3")
            {
                await _awsS3Service.DeleteFolderAsync(folderPath);
                return;
            }
            await _fileHelpers.DeleteFolder(folderPath);
            // await _awsS3Service.DeleteFolderAsync(folderPath);
        }
        ////////////////////////////////////////////////////////

        public async Task<JArray> GetChatMessagesBy1v1(int accountId1, int accountId2)
        {
            try
            {
                var chats= await _unitOfWork.ChatRepository.FindBy1v1(accountId1, accountId2);
                List<object> result = new List<object>();
                foreach (var chat in chats)
                {
                    result.Add(new
                    {
                        FromAccount = new
                        {
                            Id = chat.FromNavigation.Id,
                            FullName = chat.FromNavigation.FullName, 
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, chat.FromNavigation.Id.ToString(), "main")
                        },
                        ToAccount = new
                        {
                            Id = chat.ToNavigation.Id,
                            FullName = chat.ToNavigation.FullName,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, chat.ToNavigation.Id.ToString(), "main")
                        },
                        Chat = new
                        {
                            Id = chat.Id,
                            Message = chat.Message,
                            Created = chat.Created,
                            SenderName = chat.SenderName,
                            From = chat.From,
                            To = chat.To
                        }
                    });
                }
                return JArray.FromObject(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting chat messages: {ex.StackTrace}");
                throw new Exception("Error while getting chat messages: " + ex.Message);
            }
        }

        

        public async Task<JArray> GetChatRoomsByAccountId(int accountId)
        {
            try
            {
                var chatRooms = await _unitOfWork.ChatRepository.GetChatRoomsByAccountId(accountId);
                List<object> result = new List<object>();
                foreach(var chatroom in chatRooms)
                {
                    string folderPath = _filePathConfig.ACCOUNt_IMAGE_PATH;
                    var lastChatRoomMessage = await _unitOfWork.ChatRepository.GetLastChatBy1v1(accountId, chatroom.Id);
                    result.Add(new
                    {
                        Id = chatroom.Id,
                        Account = new
                        {
                            Id = chatroom.Id,
                            FullName = chatroom.FullName,
                            Email = chatroom.Email,
                            DateOfBirth = chatroom.DateOfBirth,
                            Address = chatroom.Address,
                            Phone = chatroom.Phone,
                            RoleId = chatroom.RoleId,
                            JobTypeId = chatroom.JobTypeId,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, chatroom.Id.ToString(), "main"),
                            IsDeactivated = chatroom.IsDeactivated,
                            CreatedAt = chatroom.CreatedAt,
                        },
                        LastChatMessage = new
                        {
                            Id = lastChatRoomMessage.Id,
                            Message = lastChatRoomMessage.Message,
                            Created = lastChatRoomMessage.Created,
                            SenderName = lastChatRoomMessage.SenderName,
                            From = lastChatRoomMessage.From,
                            To = lastChatRoomMessage.To
                        }
                        
                    });
                }

                

                return JArray.FromObject(result);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting chat rooms: {ex.Message}");
                throw new Exception("Error while getting chat room: " + ex.Message);
            }
        }
        public async Task<Chat> AddChatMessage(dynamic chatMessage)
        {
            try
            {
                var chat = new Chat
                {
                    SenderName = chatMessage.SenderName,
                    Message = chatMessage.Message,
                    From = chatMessage.From,
                    To = chatMessage.To
                };
                return await _chatRepository.CreateAsync(chat);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding chat message: " + ex.Message);
            }
        }

        public async Task<JObject> GetChatMessageById(int chatId)
        {
            try
            {
                var chat = await _unitOfWork.ChatRepository.FindByIdAsync(chatId);
                if (chat == null)
                {
                    throw new Exception("Chat message not found.");
                }else 
                {
                    Console.WriteLine($"Chat message found: {chat.Id}");
                }
                
                return JObject.FromObject(new{
                    ChatMessage = new
                    {
                        Id = chat.Id,
                        Message = chat.Message,
                        Created = chat.Created,
                        SenderName = chat.SenderName,
                        From = chat.From,
                        To = chat.To,
                        FromAccount = new
                        {
                            Id = chat.FromNavigation.Id,
                            FullName = chat.FromNavigation.FullName,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, chat.FromNavigation.Id.ToString(), "main")
                        },
                        ToAccount = new
                        {
                            Id = chat.ToNavigation.Id,
                            FullName = chat.ToNavigation.FullName,
                            ImageUrl = await GenerateImageUrl(_filePathConfig.ACCOUNt_IMAGE_PATH, chat.ToNavigation.Id.ToString(), "main")
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting chat message by id: {ex.StackTrace}");
                throw new Exception("Error while getting chat message by id: " + ex.Message);
            }
        }
    }
}
