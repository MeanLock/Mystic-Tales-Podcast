using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Architecture_1.Infrastructure.Services.AWS.S3;
using Architecture_1.Infrastructure.Services.Google.Email;
using Architecture_1.Infrastructure.Services.OpenAI._4oMini;
using Architecture_1.Infrastructure.Services.Payos;
using Architecture_1.Infrastructure.Services.Redis;
using Architecture_1.Infrastructure.Services.Consul;
using Architecture_1.Infrastructure.Services.Kafka;
using Architecture_1.Infrastructure.Configurations.Google;
using Architecture_1.Infrastructure.Configurations.Redis;
using Architecture_1.Infrastructure.Configurations.Kafka.interfaces;
using Architecture_1.Infrastructure.Configurations.Kafka;
using Consul;
using StackExchange.Redis;
using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;

namespace Architecture_1.Infrastructure.Registrations
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add service groups
            services.AddAWSServices(configuration);
            services.AddGoogleServices(configuration);
            services.AddOpenAIServices(configuration);
            services.AddPayosServices(configuration);
            services.AddRedisServices(configuration);
            services.AddConsulServices(configuration);
            services.AddKafkaServices(configuration);

            return services;
        }

        #region AWS Services
        private static IServiceCollection AddAWSServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // AWS Configuration already handled in ConfigurationRegistration
            
            // AWS Services
            services.AddScoped<AWSS3Service>();

            return services;
        }
        #endregion

        #region Google Services
        private static IServiceCollection AddGoogleServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Google Configuration already handled in ConfigurationRegistration
            
            // FluentEmail Configuration
            var googleMailConfig = configuration.GetSection("Infrastructure:Google:Mail").Get<GoogleMailConfigModel>();
            if (googleMailConfig != null)
            {
                var defaultFromEmail = googleMailConfig.FromEmail;
                
                services.AddFluentEmail(defaultFromEmail)
                    .AddRazorRenderer()
                    .AddSmtpSender(new SmtpClient(googleMailConfig.SmtpHost)
                    {
                        Port = googleMailConfig.SmtpPort,
                        Credentials = new NetworkCredential(googleMailConfig.SmtpUsername, googleMailConfig.SmtpPassword),
                        EnableSsl = googleMailConfig.EnableSsl
                    });
            }
            
            // Google Services
            services.AddScoped<FluentEmailService>();

            return services;
        }
        #endregion

        #region OpenAI Services
        private static IServiceCollection AddOpenAIServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // OpenAI Configuration already handled in ConfigurationRegistration
            
            // OpenAI Services
            services.AddScoped<OpenAI4oMiniService>();

            return services;
        }
        #endregion

        #region Payos Services
        private static IServiceCollection AddPayosServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Payos Configuration already handled in ConfigurationRegistration
            
            // Payos Services
            services.AddScoped<PayosService>();

            return services;
        }
        #endregion

        #region Redis Services
        private static IServiceCollection AddRedisServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Redis Configuration already handled in ConfigurationRegistration
            
            // Redis Connection Configuration
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = configuration.GetSection("Infrastructure:Redis:Default").Get<RedisDefaultConfigModel>();
                if (config != null)
                {
                    var options = new ConfigurationOptions
                    {
                        EndPoints = { config.ConnectionString },
                        Password = config.Password,
                        ConnectTimeout = config.ConnectTimeout,
                        SyncTimeout = config.SyncTimeout,
                        AbortOnConnectFail = config.AbortOnConnectFail,
                        ConnectRetry = config.ConnectRetry,
                        Ssl = config.UseSsl,
                        DefaultDatabase = config.DefaultDatabase
                    };
                    return ConnectionMultiplexer.Connect(options);
                }
                throw new InvalidOperationException("Redis configuration not found");
            });
            
            // Redis Services
            services.AddScoped<RedisCacheService>();

            return services;
        }
        #endregion

        #region Consul Services
        private static IServiceCollection AddConsulServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Consul Configuration already handled in ConfigurationRegistration
            
            // Consul Client Configuration
            var consulServiceHost = configuration.GetSection("Infrastructure:Consul:Service:Host").Value ?? "http://localhost:8500";
            
            services.AddSingleton<IConsulClient>(provider =>
            {
                return new ConsulClient(config =>
                {
                    config.Address = new Uri(consulServiceHost);
                });
            });
            
            // Consul Services
            services.AddHostedService<ConsulRegistrationHostedService>();

            return services;
        }
        #endregion

        #region Kafka Services
        private static IServiceCollection AddKafkaServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Kafka Configuration already handled in ConfigurationRegistration
            
            // Kafka Services
            services.AddSingleton<KafkaProducerService>();
            
            // Register KafkaConsumerService as Singleton utility service
            services.AddSingleton<KafkaConsumerService>();
            

            // Health Check
            services.AddSingleton<KafkaHealthCheckService>();
            services.AddHealthChecks()
                .AddCheck<KafkaHealthCheckService>("kafka", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, new[] { "kafka", "messaging" });

            return services;
        }
        #endregion


    }
}
