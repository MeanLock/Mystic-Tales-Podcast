using PodcastService.Infrastructure.Services.Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodcastService.Infrastructure.Services.AWS.S3;
using PodcastService.Infrastructure.Services.EmbeddingVector;
using PodcastService.Infrastructure.Services.Google.Email;
using PodcastService.Infrastructure.Services.OpenAI._4oMini;
using PodcastService.Infrastructure.Services.Payos;
using PodcastService.Infrastructure.Services.Redis;
using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using System.Net;
using System.Net.Mail;
using PodcastService.Infrastructure.Configurations.Google;
using StackExchange.Redis;
using PodcastService.Infrastructure.Configurations.Redis;
using Consul;
using PodcastService.Infrastructure.Configurations.Payos;
using Net.payOS;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.Infrastructure.Services.Audio.AcoustID;
using PodcastService.Infrastructure.Services.Audio.Hls;
using PodcastService.Infrastructure.Services.Audio.Tuning;
using PodcastService.Infrastructure.Services.Audio.Transcription;
using PodcastService.Infrastructure.Services.Consul.DistributedLock;

namespace PodcastService.Infrastructure.Registrations
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add all infrastructure services
            services.AddAWSServices(configuration);
            services.AddGoogleServices(configuration);
            services.AddEmbeddingVectorServices(configuration);
            services.AddOpenAIServices(configuration);
            services.AddPayosServices(configuration);
            services.AddRedisServices(configuration);
            services.AddConsulServices(configuration);
            services.AddKafkaServices(configuration);
            services.AddAudioServices(configuration);

            return services;
        }

        private static IServiceCollection AddAWSServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<AWSS3Base64FileService>();
            services.AddScoped<AWSS3BinaryFileService>();
            return services;
        }

        private static IServiceCollection AddGoogleServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register FluentEmail service
            services.AddScoped<FluentEmailService>();

            // Configure FluentEmail with Google Mail settings
            var googleMailConfig = configuration.GetSection("Infrastructure:Google:Mail").Get<GoogleMailConfigModel>();

            if (googleMailConfig != null &&
                !string.IsNullOrEmpty(googleMailConfig.SmtpHost) &&
                !string.IsNullOrEmpty(googleMailConfig.SmtpUsername))
            {
                services.AddFluentEmail(googleMailConfig.FromEmail)
                    .AddRazorRenderer()
                    .AddSmtpSender(new SmtpClient(googleMailConfig.SmtpHost)
                    {
                        Port = googleMailConfig.SmtpPort,
                        Credentials = new NetworkCredential(googleMailConfig.SmtpUsername, googleMailConfig.SmtpPassword),
                        EnableSsl = googleMailConfig.EnableSsl
                    });
            }

            return services;
        }

        private static IServiceCollection AddEmbeddingVectorServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<SurveyTalkEmbeddingVectorService>();
            return services;
        }

        private static IServiceCollection AddOpenAIServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<OpenAI4oMiniService>();
            return services;
        }

        private static IServiceCollection AddPayosServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register PayOS service
            services.AddScoped<PayosService>();

            // Configure PayOS client
            var payosConfig = configuration.GetSection("Infrastructure:PayOS").Get<PayosConfigModel>();

            if (payosConfig != null &&
                !string.IsNullOrEmpty(payosConfig.ClientID) &&
                !string.IsNullOrEmpty(payosConfig.APIKey) &&
                !string.IsNullOrEmpty(payosConfig.ChecksumKey))
            {
                services.AddSingleton(sp =>
                {
                    var payos = new PayOS(
                        payosConfig.ClientID,
                        payosConfig.APIKey,
                        payosConfig.ChecksumKey
                    );
                    return payos;
                });
            }

            return services;
        }

        private static IServiceCollection AddRedisServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Redis service
            services.AddScoped<RedisInstanceCacheService>();
            services.AddScoped<RedisSharedCacheService>();

            // Configure Redis connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = configuration.GetSection("Infrastructure:Redis:Default").Get<RedisDefaultConfigModel>();

                if (config != null && !string.IsNullOrEmpty(config.ConnectionString))
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

                // Fallback configuration if config is null
                return ConnectionMultiplexer.Connect("localhost:6379");
            });

            return services;
        }

        private static IServiceCollection AddConsulServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Consul hosted service
            services.AddHostedService<ConsulRegistrationHostedService>();

            // Configure Consul client
            var consulServiceHost = configuration.GetSection("Infrastructure:Consul:Service:Host").Value ?? "http://localhost:8500";

            services.AddSingleton<IConsulClient>(provider =>
            {
                return new ConsulClient(config =>
                {
                    config.Address = new Uri(consulServiceHost);
                });
            });

            // Register Consul Distributed Lock Service
            services.AddSingleton<ConsulDistributedLockService>();
            services.AddSingleton<ConsulSessionManager>();

            return services;
        }

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

        private static IServiceCollection AddAudioServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // AcoustID Audio Services
            services.AddScoped<AcoustIDAudioFingerprintGenerator>();
            services.AddScoped<AcoustIDAudioFingerprintComparator>();

            // HLS Service
            services.AddScoped<FFMegLocalHlsService>();
            services.AddScoped<FFMpegCoreHlsService>();

            // Audio Tuning Services
            services.AddScoped<EqualizerTuningService>();
            services.AddScoped<AITuningService>();
            services.AddScoped<AdvanceTuningService>();

            // Transcription Services
            services.AddScoped<AudioTranscriptionApiService>();

            return services;
        }


    }

}
