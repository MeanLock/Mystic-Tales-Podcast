using System.Threading.Tasks;
using Architecture_1.API.GraphQL.Features.Account.Types;
using Architecture_1.API.GraphQL.Features.Chat.Types;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;
using Newtonsoft.Json.Linq;
using EF = Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Chat.Queries
{
    public class ChatQuery
    {
        public ChatQuery() { }

        public async Task<List<ChatRoom1v1>> GetChatRoomsByAccountId(
            int accountId,
            [Service] ChatService chatService)
        {
            JArray chatRooms = await chatService.GetChatRoomsByAccountId(accountId);

            return chatRooms.ToObject<List<ChatRoom1v1>>();
        }
        public async Task<List<ChatMessage>> GetChatMessagesBy1v1(
            int accountId1,
            int accountId2,
            [Service] ChatService chatService)
        {
            var messages = await chatService.GetChatMessagesBy1v1(accountId1, accountId2);
            try
            {
                List<ChatMessage> chatMessagesList = messages.Select(m => {
                    dynamic chatMessage = m.ToObject<dynamic>();
                    return new ChatMessage
                {
                    Id = chatMessage.Chat.Id,
                    SenderName = chatMessage.Chat.SenderName,
                    Message = chatMessage.Chat.Message,
                    From = chatMessage.Chat.From,
                    To = chatMessage.Chat.To,
                    Created = chatMessage.Chat.Created,
                    FromAccount = new MessageAccount
                    {
                        Id = chatMessage.FromAccount.Id,
                        FullName = chatMessage.FromAccount.FullName,
                        ImageUrl = chatMessage.FromAccount.ImageUrl
                    },
                    ToAccount = new MessageAccount
                    {
                        Id = chatMessage.ToAccount.Id,
                        FullName = chatMessage.ToAccount.FullName,
                        ImageUrl = chatMessage.ToAccount.ImageUrl
                    }
                };
                }).ToList();
                Console.WriteLine($"\n\n\n\n OOOOOOOOOOOOOOOOOOOGetChatMessagesBy1v1: {chatMessagesList.Count()}");
                return chatMessagesList;


            }catch (Exception ex)
            {
                
                Console.WriteLine($"Error: {ex.StackTrace}");
                throw new Exception($"Error: {ex.StackTrace}");
            }
        }
    }

}
