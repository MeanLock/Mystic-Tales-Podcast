using Architecture_1.API.GraphQL.Features.Account.Types;
using Architecture_1.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using EF = Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Chat.Types
{
    public class ChatMessage : EF.Chat { // EF.Chat sẽ được quét để trở thành 1 type trong GraphQL bên cạnh ChatMessage
        public MessageAccount? FromAccount { get; set; } = new MessageAccount();
        public MessageAccount? ToAccount { get; set; } = new MessageAccount();

    }
    public class ChatMessageType : ObjectType<ChatMessage>
    {
        protected override void Configure(IObjectTypeDescriptor<ChatMessage> descriptor)
        {
            descriptor.Name("ChatMessage");
            descriptor.Ignore(f => f.Id);  
            
        }
    }


}
