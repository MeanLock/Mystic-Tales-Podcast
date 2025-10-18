
namespace SurveyTalkService.API.Configurations.App
{
    public static class GraphQLConfig
    {
        public static void AddAppGraphQLConfig(this WebApplication app)
        {
            app.UseWebSockets();
            
            // Đăng ký GraphQL Playground
            app.UseGraphQLPlayground();

            // Đăng ký endpoint GraphQL
            app.MapGraphQL("/graphql");

        }
    }
}
