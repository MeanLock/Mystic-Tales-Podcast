using SurveyTalkService.GraphQL.Schema.MutationGroups;

namespace SurveyTalkService.API.GraphQL.Registrations
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
            // services.AddScoped<ProductMutation>();
            // services.AddScoped<CategoryMutation>();
            // services.AddScoped<ChatMutation>();

            return services;
        }
    }
}
