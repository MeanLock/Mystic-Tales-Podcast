using Microsoft.Extensions.DependencyInjection;
using PodcastService.DataAccess.Repositories;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.DataAccess.UOW;

namespace PodcastService.DataAccess.Registrations
{
    public static class RepositoryRegistration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddDbRepositories();
            services.AddGenericRepositories();
            services.AddUnitOfWork();

            return services;
        }

        public static IServiceCollection AddDbRepositories (this IServiceCollection services) {
            // services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPodcastChannelHashtagRepository, PodcastChannelHashtagRepository>();
            services.AddScoped<IPodcastShowHashtagRepository, PodcastShowHashtagRepository>();
            services.AddScoped<IPodcastEpisodeHashtagRepository, PodcastEpisodeHashtagRepository>();
            services.AddScoped<IPodcastEpisodeIllegalContentTypeMarkingRepository, PodcastEpisodeIllegalContentTypeMarkingRepository>();
            services.AddScoped<IPodcastEpisodePublishDuplicateDetectionRepository, PodcastEpisodePublishDuplicateDetectionRepository>();
            services.AddScoped<IPodcastEpisodeListenSessionRepository, PodcastEpisodeListenSessionRepository>();
            services.AddScoped<IPodcastShowReviewRepository, PodcastShowReviewRepository>();

            return services;
        }

        public static IServiceCollection AddGenericRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
