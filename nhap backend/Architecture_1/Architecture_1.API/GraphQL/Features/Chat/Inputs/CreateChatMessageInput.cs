namespace Architecture_1.API.GraphQL.Features.Chat.Inputs
{
    public class CreateChatMessageInput
    {
        public string Message { get; set; } = string.Empty;
        public int From { get; set; }
        public int To { get; set; }
    }

    public class CreateChatMessageInputType : InputObjectType<CreateChatMessageInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateChatMessageInput> descriptor)
        {
            descriptor.Name("CreateChatMessageInput");

            descriptor.Field(f => f.Message)
                // .Name("Message")
                .Type<NonNullType<StringType>>();
            
            descriptor.Field(f => f.From)
                // .Name("From")
                .Type<NonNullType<IntType>>();
            
            descriptor.Field(f => f.To)
                // .Name("To")
                .Type<NonNullType<IntType>>();
            

        }
    }


}
