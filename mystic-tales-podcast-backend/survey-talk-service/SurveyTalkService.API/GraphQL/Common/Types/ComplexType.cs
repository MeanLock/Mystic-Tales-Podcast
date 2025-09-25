namespace SurveyTalkService.GraphQL.Common.Types
{
    public class ComplexJsonItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Values { get; set; }
    }

    public class ComplexJsonMetadata
    {
        public DateTime CreatedAt { get; set; }
        public string Source { get; set; }
    }

    public class ComplexJsonData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ComplexJsonItem> Items { get; set; }
        public ComplexJsonMetadata Metadata { get; set; }
    }

    public class ComplexJsonDataType : ObjectType<ComplexJsonData>
    {
        protected override void Configure(IObjectTypeDescriptor<ComplexJsonData> descriptor)
        {
            descriptor.Field(f => f.Title).Type<StringType>();
            descriptor.Field(f => f.Description).Type<StringType>();
            descriptor.Field(f => f.Items).Type<ListType<ObjectType<ComplexJsonItem>>>();
            descriptor.Field(f => f.Metadata).Type<ObjectType<ComplexJsonMetadata>>();
        }
    }

    public class ComplexJsonItemType : ObjectType<ComplexJsonItem>
    {
        protected override void Configure(IObjectTypeDescriptor<ComplexJsonItem> descriptor)
        {
            descriptor.Field(f => f.Id).Type<IntType>();
            descriptor.Field(f => f.Name).Type<StringType>();
            descriptor.Field(f => f.Values).Type<ListType<IntType>>();
        }
    }

    public class ComplexJsonMetadataType : ObjectType<ComplexJsonMetadata>
    {
        protected override void Configure(IObjectTypeDescriptor<ComplexJsonMetadata> descriptor)
        {
            descriptor.Field(f => f.CreatedAt).Type<DateTimeType>();
            descriptor.Field(f => f.Source).Type<StringType>();
        }
    }

}
