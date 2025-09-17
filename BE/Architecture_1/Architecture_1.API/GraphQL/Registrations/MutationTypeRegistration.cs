using Architecture_1.API.GraphQL.Features.Product.Mutations;
using Architecture_1.API.GraphQL.Features.Category.Mutations;
using Architecture_1.GraphQL.Schema.MutationGroups;
using Architecture_1.API.GraphQL.Features.Chat.Mutations;

namespace Architecture_1.API.GraphQL.Registrations
{
    public static class MutationTypeRegistration
    {
        public static IServiceCollection AddMutationTypes(this IServiceCollection services)
        {
            services.AddMutationGroupTypes();
            services.AddDbMutationTypes();

            return services;
        }

        public static IServiceCollection AddMutationGroupTypes(this IServiceCollection services)
        {
            services.AddScoped<DbMutation>();

            return services;
        }

        public static IServiceCollection AddDbMutationTypes(this IServiceCollection services)
        {
            services.AddScoped<ProductMutation>();
            services.AddScoped<CategoryMutation>();
            services.AddScoped<ChatMutation>();

            return services;
        }
    }
}
