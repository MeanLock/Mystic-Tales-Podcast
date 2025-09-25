namespace SurveyTalkService.GraphQL.Common.Types
{
    public class NhapType : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("Nhap");
            descriptor.Description("Loại đối tượng Nhap");

            descriptor
                .Field("id")
                .Type<NonNullType<IntType>>()
                .Resolve(context => context.Parent<object>()?.GetType().GetProperty("id")?.GetValue(context.Parent<object>()))
                .Description("ID của Nhap");

            descriptor
                .Field("tenTaoNe")
                .Type<StringType>()
                .Resolve(context => context.Parent<object>()?.GetType().GetProperty("tenTaoNe")?.GetValue(context.Parent<object>()))
                .Description("Tên Tao Ne");
        }
    }


}
