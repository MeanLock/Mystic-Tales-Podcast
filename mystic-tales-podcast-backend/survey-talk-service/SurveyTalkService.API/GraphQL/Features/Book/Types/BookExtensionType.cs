namespace SurveyTalkService.API.GraphQL.Features.Book.Types
{

    //** ObjectType để Configure Type Book đã được quét trước đó trong schema
    public class BookExtensionTypeExtension : ObjectTypeExtension<Book>
    {
        protected override void Configure(IObjectTypeDescriptor<Book> descriptor)
        {
            descriptor.Name("Book_Configured_in_ObjectType"); // Cập nhật tên để khớp với schema
            descriptor.Field(b => b.Title).Name("Title_Configured").Type<StringType>();

            // thêm các field mới vào type
            descriptor.Field("AddFieldHere_string")
                .Resolve(context => "This is a book about GraphQL.")
                .Type<StringType>();
        }
    }

    //** ExtendObjectType để mở rộng 1 Type đã được tạo trước đó trong schema để thêm các field mới vào (các field được định nghĩa trong class nơi dùng directive [ExtendObjectType])
    // [ExtendObjectType(typeof(Book))]                             // Cách 1: ExtendObjectType cho type Book với class đối tượng cụ thể
    [ExtendObjectType("Book_Configured_in_ObjectType")]             // Cách 2: ExtendObjectType cho type "Book_Configured_in_ObjectType" (trước đó tên Type là "Book" nhưng Configure() trong BookType đã thay đổi nó thành "Book_Configured_in_ObjectType") là tên type trong Schema khi chạy ứng dụng
    public class BookExtension
    {

        public string summary => "This is a book about GraphQL.";

        public int Price { set; get; }              // field này sẽ được thêm vào type mặc dù không có giá trị trả về nhưng nó trả về giá trị mặc định của kiểu int 

        [GraphQLIgnore]
        public string Description { set; get; }     // field này sẽ không được thêm vào type vì có attribute GraphQLIgnore

        public int PostalCode;                      // field này sẽ không được thêm vào type vì không có giá trị trả về

        public string DoSomething()
        {
            Console.WriteLine("Doing something...");
            return "Operation completed";
        }

        public void DoSomething_void()              // method này sẽ không được thêm vào type vì là void
        {
            Console.WriteLine("Doing something...");
        }


    }
}
