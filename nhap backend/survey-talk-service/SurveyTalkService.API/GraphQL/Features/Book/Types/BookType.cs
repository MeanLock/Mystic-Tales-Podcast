namespace SurveyTalkService.API.GraphQL.Features.Book.Types
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublishedDate { get; set; }
    }

    public class BookType : ObjectType<Book>
    {
        protected override void Configure(IObjectTypeDescriptor<Book> descriptor)
        {
            descriptor.Name("Book_Configured_in_ObjectType"); // Đặt tên type trong schema
            descriptor.Field(b => b.Title).Name("Title_Configured").Type<StringType>();
        }
    }


    //** ObjectType để Configure Type Book đã được quét trước đó trong schema
    //public class BookType : ObjectType<Book>
    //{
    //    protected override void Configure(IObjectTypeDescriptor<Book> descriptor)
    //    {
    //        descriptor.Name("Book_Configured_in_ObjectType"); // Đặt tên type trong schema
    //        descriptor.Field(b => b.Id).Name("Id_Configured").Type<IntType>(); // Đặt tên field trong schema
    //        descriptor.Field(b => b.Title).Name("Title_Configured").Type<StringType>();
    //        descriptor.Field(b => b.Author).Name("Author_Configured").Type<StringType>();
    //        descriptor.Field(b => b.PublishedDate).Name("PublishedDate_Configured").Type<DateTimeType>();

    //    }
    //}
}
