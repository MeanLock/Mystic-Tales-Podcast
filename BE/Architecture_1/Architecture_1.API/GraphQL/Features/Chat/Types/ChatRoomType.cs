using Architecture_1.API.GraphQL.Features.Account.Types;
using Architecture_1.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using EF = Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Chat.Types
{
    public class ChatRoom1v1
    {
        public int Id { get; set; } = default!;
        public AccountExtended Account { get; set; } = default!;
        public EF.Chat? LastChatMessage { get; set; } = default!;
    } 

    public class ChatRoomType : ObjectType<ChatRoom1v1>
    {
        protected override void Configure(IObjectTypeDescriptor<ChatRoom1v1> descriptor)
        {
            descriptor.Name("ChatRoom1v1");
            descriptor.Field(f => f.Id)
                .Type<NonNullType<IntType>>()
                .Description("The unique identifier for the chat room.");
            descriptor.Field(f => f.Account)
                .Description("The account associated with the chat room.");
            descriptor.Field(f => f.LastChatMessage)
                .Description("The last chat message in the chat room.");
        }
    }

}
