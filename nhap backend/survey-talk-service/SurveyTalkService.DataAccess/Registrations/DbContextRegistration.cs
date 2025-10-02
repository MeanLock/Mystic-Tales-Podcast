using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.DataAccess.Data;

namespace SurveyTalkService.DataAccess.Registrations
{
    public static class DbContextRegistration
    {
        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            // SQL Server
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("SQLSERVER_DefaultConnectionString");
                options.UseSqlServer(connectionString);  // 1. Sử dụng SQL Server
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);  // 2. Bật AsNoTracking cho toàn bộ truy vấn
                options.UseLazyLoadingProxies(false);  // 3. Kích hoạt Lazy Loading
            });

            // PostgreSQL
            services.AddDbContext<PostgresDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("POSTGRESQL_DefaultConnectionString");
                options.UseNpgsql(connectionString, o => o.UseVector());
                // options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                // options.UseLazyLoadingProxies(false);
            });
            return services;
        }
    }
}
