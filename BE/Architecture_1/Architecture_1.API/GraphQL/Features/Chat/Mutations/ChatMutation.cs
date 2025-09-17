using Architecture_1.API.GraphQL.Features.Chat.Inputs;
using Architecture_1.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using EF = Architecture_1.DataAccess.Entities;
using HotChocolate.Subscriptions;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;
using Architecture_1.DataAccess.Entities;
using Newtonsoft.Json.Linq;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Architecture_1.API.GraphQL.Features.Chat.Types;
using Architecture_1.API.GraphQL.Features.Chat.Subscriptions;

namespace Architecture_1.API.GraphQL.Features.Chat.Mutations
{
    public class ChatMutation
    {
        public async Task<bool> SendMessageAsync(
        CreateChatMessageInput chatMessageInput,
        [Service] ChatService chatService,
        [Service] AccountService accountService,
        [Service] ITopicEventSender sender)
        {
            var account = await accountService.GetExistAccount(chatMessageInput.From);

            dynamic chat = new EF.Chat
            {
                SenderName = account.FullName,
                Message = chatMessageInput.Message,
                From = chatMessageInput.From,
                To = chatMessageInput.To
            };



            EF.Chat chatNew = await chatService.AddChatMessage(chat);
            ChatMessage payload = (await chatService.GetChatMessageById(chatNew.Id))["ChatMessage"].ToObject<ChatMessage>();

            if (payload == null)
            {
                return false;
            }

            List<ChatRoom1v1> chatRooms_from = (await chatService.GetChatRoomsByAccountId(chatMessageInput.From)).ToObject<List<ChatRoom1v1>>();
            List<ChatRoom1v1> chatRooms_to = (await chatService.GetChatRoomsByAccountId(chatMessageInput.To)).ToObject<List<ChatRoom1v1>>();

            

            await sender.SendAsync($"ChatRoom_User_{chatMessageInput.From}", chatRooms_from);
            await sender.SendAsync($"ChatRoom_User_{chatMessageInput.To}", chatRooms_to); 
            // await sender.SendAsync($"ChatRoom_User_{chatMessageInput.From}", new ChatRoomUpdatePayload
            // {
            //     AccountId = chatMessageInput.From,
            //     ChatRooms = chatRooms_from
            // });

            // await sender.SendAsync($"ChatRoom_User_{chatMessageInput.To}", new ChatRoomUpdatePayload
            // {
            //     AccountId = chatMessageInput.To,
            //     ChatRooms = chatRooms_to
            // });

            // Gửi cho cả 2 user_id như SignalR group
            await sender.SendAsync($"Chat_User_{chatMessageInput.From}", payload);
            await sender.SendAsync($"Chat_User_{chatMessageInput.To}", payload);

            return true;
        }
    }
}
