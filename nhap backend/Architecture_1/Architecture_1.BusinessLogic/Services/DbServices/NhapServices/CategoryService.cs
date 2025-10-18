
using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.BusinessLogic.Services.DbServices.NhapServices
{
    public class CategoryService
    {
        public AppDbContext _context;


        public CategoryService(AppDbContext context) 
        {
            _context = context;
        }

        public List<Category> GetAllCategories()
        {
            try
            {
                Console.WriteLine("\n\n ALO:" + _context.Categories.ToList().Count);
                return _context.Categories.ToList();
            }catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                Console.WriteLine("Lỗi khi lấy danh sách danh mục: " + ex.Message);
                Console.WriteLine("\n" + ex.StackTrace);
                
                throw new Exception("Lỗi khi lấy danh sách danh mục", ex);
            }
        }

        public Category GetCategoryById(int id)
        {
            return _context.Categories.Find(id);
        }


        // Thêm mới danh mục
        public void AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        // Cập nhật danh mục
        public void UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }
    }
}
