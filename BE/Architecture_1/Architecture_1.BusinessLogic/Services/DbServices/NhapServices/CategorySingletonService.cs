using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Architecture_1.BusinessLogic.Services.DbServices.NhapServices
{
    public class CategorySingletonService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public CategorySingletonService(IServiceScopeFactory serviceScopeFactory) 
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public List<Category> GetAllCategories()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Dùng dbContext tại đây
                return dbContext.Categories.ToList();
            }
            
        }
    }
}
