

using Architecture_1.API.GraphQL.Features.Category.Inputs;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;
using EF = Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Category.Mutations
{
    public class CategoryMutation
    {
        private readonly CategoryService _categoryService;

        public CategoryMutation(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Mutation thêm mới danh mục
        public EF.Category AddCategory(
            [Service] CategoryService categoryService,
            CreateCategoryInput createCategoryInput
            )
        {
            var category = new EF.Category
            {
                CategoryName = createCategoryInput.Name,
                Description = createCategoryInput.Description,
                CreatedAt = DateTime.Now
            };

            categoryService.AddCategory(category); // Giả sử có phương thức AddCategory trong CategoryService

            return category;
        }

        // Mutation cập nhật thông tin danh mục
        public EF.Category UpdateCategory(
            [Service] CategoryService categoryService,
            int categoryId,
            string categoryName,
            string? description
            )
        {
            var existingCategory = categoryService.GetCategoryById(categoryId);
            if (existingCategory == null) throw new Exception("Category not found");

            existingCategory.CategoryName = categoryName;
            existingCategory.Description = description;
            existingCategory.CreatedAt = existingCategory.CreatedAt ?? DateTime.Now;

            categoryService.UpdateCategory(existingCategory); // Giả sử có phương thức UpdateCategory trong CategoryService

            return existingCategory;
        }
    }
}
