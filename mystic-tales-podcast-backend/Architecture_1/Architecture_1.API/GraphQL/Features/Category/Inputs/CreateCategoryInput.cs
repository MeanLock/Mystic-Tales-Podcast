namespace Architecture_1.API.GraphQL.Features.Category.Inputs
{
    public class CreateCategoryInput
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateCategoryInputType : InputObjectType<CreateCategoryInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateCategoryInput> descriptor)
        {
            descriptor.Name("CreateCategoryInput");

            descriptor.Field(f => f.Name)
                // .Name("name")
                .Type<NonNullType<StringType>>();

            descriptor.Field(f => f.Description)
                // .Name("description")
                .Type<NonNullType<StringType>>();
        }
    }


}
