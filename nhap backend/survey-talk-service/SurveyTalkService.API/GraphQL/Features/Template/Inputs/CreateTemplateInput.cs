namespace SurveyTalkService.API.GraphQL.Features.Template.Inputs
{
    public class CreateTemplateInput
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }

    public class CreateTemplateInputType : InputObjectType<CreateTemplateInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateTemplateInput> descriptor)
        {
            descriptor.Name("CreateTemplateInput");

            descriptor.Field(f => f.name)
                .Name("name")
                .Type<NonNullType<StringType>>();

            descriptor.Field(f => f.description)
                .Name("description")
                .Type<NonNullType<StringType>>();
        }
    }


}
