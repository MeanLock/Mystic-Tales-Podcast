using Architecture_1.API.GraphQL.Features.Product.Queries;
using Architecture_1.API.GraphQL.Features.Category.Queries;
using Architecture_1.API.GraphQL.Features.Json.Queries;
using Architecture_1.GraphQL.Schema.QueryGroups;
using Architecture_1.API.GraphQL.Features.Book.Queries;
using Architecture_1.DataAccess.Entities;
using Architecture_1.API.GraphQL.Features.Chat.Queries;

namespace Architecture_1.API.GraphQL.Registrations
{
    public static class QueryTypeRegistration
    {
        public static IServiceCollection AddQueryTypes(this IServiceCollection services)
        {
            services.AddQueryGroupTypes();
            services.AddDbQueryTypes();
            services.AddJsonQueryTypes();

            return services;
        }

        public static IServiceCollection AddQueryGroupTypes(this IServiceCollection services)
        {
            services.AddScoped<DbQuery>();
            services.AddScoped<JsonQuery>();

            return services;
        }

        public static IServiceCollection AddDbQueryTypes(this IServiceCollection services)
        {
            services.AddScoped<ProductQuery>();
            services.AddScoped<CategoryQuery>();
            services.AddScoped<BookQuery>();
            services.AddScoped<ChatQuery>();

            return services;
        }

        public static IServiceCollection AddJsonQueryTypes(this IServiceCollection services)
        {
            services.AddScoped<NhapJson_1_Query>();   
            services.AddScoped<NhapJson_2_Query>(); 

            return services;
        }
    }
}
