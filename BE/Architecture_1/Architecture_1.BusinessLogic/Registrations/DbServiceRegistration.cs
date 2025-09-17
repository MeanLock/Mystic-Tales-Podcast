using Microsoft.Extensions.DependencyInjection;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices;
using Architecture_1.BusinessLogic.Services.DbServices.RequestServices;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;

namespace Architecture_1.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<AccountService>();

            services.AddScoped<FacilityService>();
            services.AddScoped<FacilityItemService>();

            services.AddScoped<MajorService>();
            services.AddScoped<MajorAssignmentService>();
            services.AddScoped<MajorServicesService>();

            services.AddScoped<TaskRequestService>();
            services.AddScoped<ServiceRequestService>();

            // Nhap Services
            services.AddScoped<CategoryService>();
            services.AddSingleton<CategorySingletonService>();
            services.AddScoped<CategoryNoInjectScopedService>();
            services.AddScoped<ProductService>();
            services.AddScoped<ChatService>();




            return services;
        }
    }
}
