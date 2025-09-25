using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;
using EF = Architecture_1.DataAccess.Entities;
using System.Data;
using Architecture_1.DataAccess.Data;
using Architecture_1.GraphQL.Schema.MutationGroups;
using Architecture_1.GraphQL.Schema.QueryGroups;

namespace Architecture_1.API.GraphQL.Features.Category.Queries
{
    public class CategoryQuery
    {
        private readonly AppDbContext _context;
        public CategoryService _categoryService { get; set; }
        public CategorySingletonService _categorySingletonService { get; set; }
        public CategoryNoInjectScopedService _categoryNoInjectScopedService;

        // Constructor nhận CategoryService từ DI container
        public CategoryQuery(AppDbContext context, CategoryService categoryService, CategorySingletonService categorySingletonService, CategoryNoInjectScopedService categoryNoInjectScopedService)
        {
            _context = context;
            _categoryService = categoryService;
            _categorySingletonService = categorySingletonService;
            _categoryNoInjectScopedService = categoryNoInjectScopedService;
        }


        public List<EF.Category> GetAllCategories_DbContext()
        {

            try
            {
                var categories = _context.Categories.ToList();
                Console.WriteLine("DbContext hoạt động bình thường, số lượng: " + categories.Count);
                return categories;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("\n\nDbContext đã bị dispose: " + ex.Message);
                return new List<EF.Category>();
            }


        }


        // Query lấy tất cả danh mục
        public List<EF.Category> GetAllCategories_Disposable() => _categoryService.GetAllCategories();
        public List<EF.Category> GetAllCategories([Service] CategoryService categoryService) => categoryService.GetAllCategories();

        public List<EF.Category> GetAllCategoriesSingleton()
        {
            // Chỉ để test xem có hoạt động không thôi
            return _categorySingletonService.GetAllCategories();
        }


        //** Authorize dạng tách bên dưới sẽ tương đương với dạng gộp [Authorize(Roles = new []{"manager","sale staff"}, Policy = "Require_Manager_ID_1")], lợi ích khi tách là dễ đọc hơn và in 2 dòng thông báo riêng biệt thay vì là 1 dòng như dang gộp 
        //** phải dùng Authorize của HotChocolate, không dùng Authorize của Microsoft.AspNetCore.Authorization
        [Authorize(Roles = new[] { "Campus Manager", "Facility Major Head", "Campus Member" })]
        [Authorize(Policy = "RequireRootEmail")]
        public string GetHello()
        {
            return _categoryNoInjectScopedService.Hello();
        }

        // Query lấy danh mục theo ID
        public EF.Category GetCategoryById(int id) => _categoryService.GetCategoryById(id);
    }

}
