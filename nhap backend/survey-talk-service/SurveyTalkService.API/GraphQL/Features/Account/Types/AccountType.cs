using Microsoft.EntityFrameworkCore;

using EF = SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.API.GraphQL.Features.Account.Types
{
    public class AccountExtended : EF.Account
    {
        public string ImageUrl { get; set; } = string.Empty;

    }
    public class AccountType : ObjectType<AccountExtended>
    {
        protected override void Configure(IObjectTypeDescriptor<AccountExtended> descriptor)
        {
            descriptor.Name("AccountExtended");
            descriptor.Field(f => f.Id)
                .Name("id")
                .Type<NonNullType<IntType>>()
                .Description("The unique identifier for the account.");
            descriptor.Field(f => f.ImageUrl)
                .Name("imageUrl")
                .Type<NonNullType<StringType>>()
                .Description("The URL of the account's image.");
        }
    }


}
