using Architecture_1.API.GraphQL.Features.Account.Types;
using Architecture_1.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using EF = Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Chat.Types
{
    public class MessageAccount {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
    public class MessageAccountType : ObjectType<MessageAccount>
    {
        protected override void Configure(IObjectTypeDescriptor<MessageAccount> descriptor)
        {
            descriptor.Name("MessageAccount");
            
        }
    }


}
