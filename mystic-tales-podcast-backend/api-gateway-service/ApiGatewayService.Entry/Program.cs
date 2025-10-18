
using ApiGatewayService.Infrastructure;
using ApiGatewayService.Common;
using ApiGatewayService.API.Configurations.App;

namespace ApiGatewayService.Entry
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Layers
            builder.Services.AddCommonLayer();
            builder.Services.AddInfrastructureLayer(builder.Configuration);

            builder.Services.AddControllers();
            
            // Add HttpClient for Gateway Controller
            builder.Services.AddHttpClient();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // YARP Reverse Proxy - Handle: /api/{serviceName}/{**catch-all}
            app.MapReverseProxy();

            // Manual Proxy Controller - Handle: /api/manual-proxy/{serviceName}/{**path}
            app.MapControllers();

            app.UseAppHealthCheckConfig();

            app.Run();
        }
    }
}