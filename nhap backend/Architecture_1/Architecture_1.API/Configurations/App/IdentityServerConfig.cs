using Architecture_1.DataAccess.Seeders;
using Architecture_1.DataAccess.Seeders.IdentityServer;

namespace Architecture_1.API.Configurations.App
{
    public static class IdentityServerConfig
    {
        public static void AddAppIdentityServerConfig(this WebApplication app, string[] args)
        {
            app.UseIdentityServer();

            if (app.Environment.IsDevelopment() && args.Contains("--seed"))
            {
                using var scope = app.Services.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<ConfigurationSeeder>();
                Task.Run(async () => await seeder.SeedAsync()).Wait();
                Console.WriteLine("Seeding completed successfully!");
            }
        }

    }
}
