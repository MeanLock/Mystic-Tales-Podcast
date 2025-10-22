
using ModerationService.API.GraphQL.Features.Json.Queries;
using ModerationService.GraphQL.Schema.QueryGroups;
using ModerationService.API.GraphQL.Features.Book.Queries;

namespace ModerationService.API.GraphQL.Registrations
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
            services.AddScoped<BookQuery>();
            // services.AddScoped<ProductQuery>();
            // services.AddScoped<CategoryQuery>();
            // services.AddScoped<ChatQuery>();

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
