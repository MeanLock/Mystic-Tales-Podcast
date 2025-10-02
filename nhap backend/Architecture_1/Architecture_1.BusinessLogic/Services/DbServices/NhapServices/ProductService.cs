using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Architecture_1.BusinessLogic.Services.DbServices.NhapServices
{
    public class ProductService
    {
        public AppDbContext _context;

       private readonly IServiceProvider _serviceProvider;

        public ProductService(
            AppDbContext context,
            IServiceProvider serviceProvider
        ) 
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public string str => "Hello";


        public void sayHello()
        {
            Console.WriteLine("Hello 1 from ProductService");
        }
        public List<Product> GetAllProducts()
        {
            List<Product> productList = _context.Products.ToList();
            Console.WriteLine(productList.Count);
            return productList;
        }


        //public List<Product> GetAllProducts(AppDbContext context)
        //{
        //    List<Product> productList = context.Products.ToList();
        //    Console.WriteLine(productList.Count);
        //    return productList;
        //}


        //** trả về kiểu IQueryable , giải thích: IQueryable sẽ trả về query chưa thực thi ngay lập tức, nó sẽ thực thi sau này khi gọi hàm ToList() hay FirstOrDefault()... Ví dụ GetAllProducts_IQueryable().Where(p => p.Price > 100).ToList() 
        public IQueryable<Product> GetAllProducts_IQueryable()
        {
            try
            {
                // Trả về IQueryable<Product> thay vì List<Product>
                return _context.Products;
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Error fetching products: {ex.Message}");
                return Enumerable.Empty<Product>().AsQueryable(); // Trả về IQueryable rỗng
            }
        }


        public Product GetProductById(int id) {
            return _context.Products.Find(id);
        }

        // Thêm mới sản phẩm
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        // Cập nhật sản phẩm
        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        // Xóa sản phẩm
        public void DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

    }
}
