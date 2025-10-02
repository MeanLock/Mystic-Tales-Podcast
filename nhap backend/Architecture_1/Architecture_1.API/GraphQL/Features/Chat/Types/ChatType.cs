using Microsoft.EntityFrameworkCore;

using EF = Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Chat.Types
{

    public class ChatType : ObjectTypeExtension<EF.Chat>
    {
        protected override void Configure(IObjectTypeDescriptor<EF.Chat> descriptor)
        {
            descriptor.Name("Chat"); 
            
        }
    }


}
