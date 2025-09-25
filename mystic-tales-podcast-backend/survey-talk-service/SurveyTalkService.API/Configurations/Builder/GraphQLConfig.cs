using SurveyTalkService.API.GraphQL.Extensions;
using SurveyTalkService.GraphQL.Schema;
using System.Reflection;
using SurveyTalkService.API.GraphQL.Registrations;


namespace SurveyTalkService.API.Configurations.Builder
{
    public static class GraphQLConfig
    {
        public static void AddBuilderGraphQLConfig(this WebApplicationBuilder builder)
        {
            //// Đăng kí các Query
            //builder.Services.AddScoped<ProductQuery>();
            //builder.Services.AddScoped<CategoryQuery>();

            //builder.Services.AddScoped<NhapJson_1_Query>();   // class này trả về kiểu JSON mà được định nghĩa rồi qua class ComplexJsonData (lớp ComplexJsonData này không cần AddType vì đây là lớp đối tượng thông thường và có thể được quét tự động bới HotChocolate để tạo type cho nó)
            //builder.Services.AddScoped<NhapJson_2_Query>();   // class này trả về kiểu JSON được tạo từ ScalarType là JsonScalarType (phải đăng kí trong AddType bên dưới để có thể apply)

            //// Đăng kí các Mutation
            //builder.Services.AddScoped<ProductMutation>();
            //builder.Services.AddScoped<CategoryMutation>();

            //// Đăng kí Query Group (là group tập hợp 1 nhóm các query lại với nhau để dễ phân loại ví dụ: DbQuery, AbcQuery)
            //builder.Services.AddScoped<DbQuery>();
            //builder.Services.AddScoped<JsonQuery>();

            //// Đăng kí Mutation Group (là group tập hợp 1 nhóm các mutation lại với nhau để dễ phân loại ví dụ: DbMutation, AbcMutation)
            //builder.Services.AddScoped<DbMutation>();

            builder.Services.AddQueryTypes(); // Đăng ký các QueryType
            builder.Services.AddMutationTypes(); // Đăng ký các MutationType
            builder.Services.AddSubscriptionTypes(); // Đăng ký các SubscriptionType 

            builder.Services.AddGraphQLServer()
                .AddAuthorization()             // Thêm Authorization cho GraphQL
                .AddQueryType<Query>()          // Thêm QueryType
                .AddMutationType<Mutation>()    // Thêm MutationType 
                .AddSubscriptionType<Subscription>() // Thêm SubscriptionType  
                // .AddTypeExtension<ChatRoomSubscription>()               
                //.AddTypes(
                //    typeof(NhapType),
                //    typeof(JsonDocumentType),     // Thêm lớp scalar type custom (để cho phép type JSON), vì kiểu JsonDocument không được quét tự động vì đây không phải 1 scalar type mặc định, nên ta tạo ra JsonDocumentType để tạo type này trong schema
                //    typeof(JTokenType)         // Thêm lớp scalar type custom (để cho phép type JSON), vì kiểu JToken không được quét tự động vì đây không phải 1 scalar type mặc định, nên ta tạo ra JTokenType để tạo type này trong schema
                //    // typeof(BookType),
                //    // typeof(BookExtension)       // Thêm lớp ExtendObjectType để nó có thể apply vào type Book trong schema, nếu không add thì ExtendObjectType sẽ không có tác dụng
                //    // typeof(ProductQuery)
                //)
                .AddGraphQLTypesFromAssemblies(
                    Assembly.GetExecutingAssembly()
                )
                .AddGraphQLExtensionsFromAssemblies(
                    Assembly.GetExecutingAssembly()
                )
                .AddGraphQLInputTypesFromAssemblies(
                    Assembly.GetExecutingAssembly()
                )
                .AddFiltering()
                .AddSorting()
                .ModifyRequestOptions(o => o.IncludeExceptionDetails = true)
                .AddInMemorySubscriptions()
                // .SetFieldNamingConvention(FieldNamingConventions.Original);
                ;


        }
        
         

    }
}
