using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;
using EF = Architecture_1.DataAccess.Entities;
using Architecture_1.DataAccess.Data;
using Architecture_1.GraphQL.Schema.QueryGroups;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.GraphQL.Features.Product.Queries
{
    public class ProductQuery
    {
        public AppDbContext _context;                        // AddDbContext không thể { get; set; } được, vì nó không phải là một kiểu dữ liệu

        // private readonly ProductService _productService;

        public ProductService _productService { get; set; } // type này để có thể xuất hiện trong Type ProductQuery thì phải có gì đó trả về , ví dụ thêm {get; set;} ở sau
        public ProductQuery(AppDbContext context, ProductService productService)
        {
            _context = context;
            _productService = productService;
        }


        //** Cách 1: khi ProductService không return (tức { get; }) nếu ta sử dụng [Service]
        // về cơ bản mọi Injection trong HotChocolate đều sẽ trở thành singleton, điều này sẽ áp dụng được cho các Scoped service thông thường ngoại trừ các service chỉ hổ trợ Scoped hoặc Transient như DbContext
        // ở đây _productService từ đầu là 1 scoped service, và nó sẽ trở thành singleton khi được inject vào ProductQuery
        // vì ProductService inject DbContext vào constructor của nó, và DbContext là scoped service
        // => _productService (singleton) sẽ không thể sử dụng DbContext (scoped) ở lần gọi thứ 2 , vì DbContext đã bị dispose sau lần gọi đầu tiên
        // => Sử dụng [Service] để inject lại ProductService vào GetAllProducts() thì nó sẽ tạo ra 1 instance mới của ProductService với DbContext mới, lúc này dbContext sẽ được khởi tạo lại
        //* CÓ THỂ DÙNG addDbContextFactory() để đăng ký DbContext với DI, và sử dụng nó để khởi tạo mới DbContext trong mỗi lần dùng thay vì phụ thuộc hoàn toàn vào Injection ban đầu
        [UseFiltering]  // cho phép Filter trong query của client (allProducts(where: { price: { gt: 200 } }))
        [UseSorting]    // cho phép Sorting trong query của client (allProducts(order: { price: DESC }))
                        //** Filter và Sorting trong query: allProducts(where: { price: { gt: 200 } }, order: { price: DESC })
                        //** FilterInput sẽ tự động quét các thuộc tính của Product và cả các thuộc tính quan hệ của nó (nếu có) để tạo ra các trường tương ứng trong FilterInput và các FilterInputType khác ứng với các field quan hệ, ví dụ bạn đầu ta có ProductFilterInput -> tạo thêm CategoryFilterInput, BrandFilterInput, ... tương ứng với các field quan hệ của Product  
        public List<EF.Product> GetAllProducts([Service] ProductService productService)
        {
            try
            {
                productService.sayHello();
                return productService.GetAllProducts();
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Error fetching products: {ex.Message}");
                return new List<EF.Product>(); // Hoặc throw một exception cụ thể
            }
        }


        //** Cách 2: khi ProductService return (tức { get; }) nếu ta sử dụng [Service] DbConext để truyền tham số cho GetAllProducts() Type ProductService (vì _productService có return nên mới có Type ProductService trong schema)
        //[UseFiltering]  // cho phép Filter trong query của client (allProducts(where: { price: { gt: 200 } }))
        //[UseSorting]    // cho phép Sorting trong query của client (allProducts(order: { price: DESC }))
        ////** Filer và Sorting trong query: allProducts(where: { price: { gt: 200 } }, order: { price: DESC })
        //public List<Product> GetAllProducts([Service] AppDbContext context)
        //{
        //    try
        //    {
        //        _productService.sayHello();
        //        return _productService.GetAllProducts(context);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log lỗi chi tiết
        //        Console.WriteLine($"Error fetching products: {ex.Message}");
        //        return new List<Product>(); // Hoặc throw một exception cụ thể
        //    }
        //}




        // Query lấy sản phẩm theo ID
        public EF.Product GetProductById([Service] ProductService productService, int id) =>
            productService.GetProductById(id);
    }

}
