using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.DataAccess.Data;

namespace SagaOrchestratorService.DataAccess.Registrations
{
    public static class DbContextRegistration
    {
        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("SQLSERVER_DefaultConnectionString");
                Console.WriteLine($"Connection String: {connectionString}");
                options.UseSqlServer(connectionString);  // 1. Sử dụng SQL Server
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);  // 2. Bật AsNoTracking cho toàn bộ truy vấn
                options.UseLazyLoadingProxies(false);  // 3. Kích hoạt Lazy Loading
            });
            return services;
        }
    }
}
