using UserService.API.Configurations.App;
using UserService.API.Configurations.Builder;
using UserService.BusinessLogic;
using UserService.Common;
using UserService.DataAccess;
using UserService.Infrastructure;

namespace UserService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Layers
            builder.Services.AddCommonLayer();
            builder.Services.AddBusinessLogicLayer();
            builder.Services.AddInfrastructureLayer(builder.Configuration);
            builder.Services.AddDataAccessLayer(builder.Configuration);


            // appConf
            builder.AddBuilderDefaultConfig();
            builder.AddBuilderCorsConfig();

            // authConf
            builder.AddBuilderAuthenticationConfig();
            builder.AddBuilderAuthorizationConfig();

            // graphQLConf
            builder.AddBuilderGraphQLConfig();

            // signalRConf
            builder.AddBuilderSignalRConfig();

            // healthCheckConf
            builder.AddBuilderHealthCheckConfig(); // [CONSUL] Register health check endpoint


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            // builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.AddAppCorsConfig();

            app.AppAppDefaultConfig();

            app.AddAppMiddlewareConfig();

            app.AddSignalRConfig();

            app.MapControllers();

            app.AddAppGraphQLConfig();

            app.AddAppIdentityServerConfig(args);

            app.UseAppHealthCheckConfig(); // [CONSUL] Register health check endpoint

            app.Run();
        }
    }
}
